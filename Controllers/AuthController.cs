using Database.Core;
using Database.Core.Enums;
using Database.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Api.Entities;
using Api.Enums;
using Api.Models;
using Api.Library;
using Api.Repositories;
using Mapster;
using System.Drawing;
using System.IO;
using System.Text;
using System.Drawing.Imaging;

namespace Api.Controllers;

/// <summary>
/// 認證API
/// </summary>
[Authorize]
public class AuthController : ActionControllerBase<AccountRepository, Account>
{
    private static readonly SortedList<string, long> CaptchaHashCodes = new();

    /// <summary>
    /// ToModel
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="orgcode"></param>
    /// <returns></returns>
    private UserLoginResult ToModel(Account entity, string orgcode)
    {
        var model = entity.Adapt<UserLoginResult>();
        if (DateTime.Now > entity.PermitChangeTime || entity.LastLoginTime == null)
            model.IsFirstLogin = true;
        model.Teken = TokenManager.CreateToken(entity.No, "");
        return model;
    }

    /// <summary>
    /// 帳號登入
    /// </summary>
    /// <param name="body">登入帳號密碼</param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<UserLoginResult>> Login([FromBody] UserLoginModel body)
    {
        //if (!CheckCaptcha(body.CaptchaHashCode, false))
        //    return BadRequest("無效的驗證碼!");

        if (!ModelState.IsValid)
            ThrowValidationException();
        var errMsg = "";

        var account = Repository.DataSet.FirstOrDefault(a => a.No == body.Name);
        if (account == null)
            return NotFound("無此帳號");

        if (account.TryCount >= 3)
        {
            var difference = DateTime.Now - account.UpdateTime!.Value;
            if (difference.Minutes <= 15)
                return BadRequest("登入密碼錯誤已達三次，本帳戶將暫停登入；請稍候15分鐘再試!");
            account.Status = 100;
            account.TryCount = 0;
            account.UpdateTime = DateTime.Now;
            await Repository.UpdateAsync(account);
        }
        if (string.IsNullOrEmpty(body.Name) || string.IsNullOrEmpty(body.Permit))
        {
            errMsg = "帳號或密碼錯誤！";
            await AuditEventService.Add(AuditEventType.LoginFail, body, body.Name, Request.Path);
            return Unauthorized(errMsg);
        }
        if (account.Status == 0) //被停用判斷
        {
            var diff = DateTime.Now - account.UpdateTime!.Value;
            if (diff.Days > 92)
                return BadRequest("本帳戶超過三個月以上未使用，已暫停登入請洽系統管理員!");
            return BadRequest("本帳戶已停用登入，請洽系統管理員!");
        }

        var hashPermit = TokenManager.ToHash(body.Permit);

        if (account.Status == 1)
            return NotFound("此帳號已停用");

        if (account.HashPermit != hashPermit)
        {
            if (account.IsSuper == false)
            {
                account.TryCount += 1;
                if (account.TryCount >= 3)
                {
                    account.Status = (short) AccountStatus.Pending;
                    account.UpdateTime = DateTime.Now;
                }

                await Repository.UpdateAsync(account);
            }

            errMsg = "登入密碼錯誤!";
            await AuditEventService.Add(AuditEventType.LoginFail, body, account.No, Request.Path);
            return BadRequest(errMsg);
        }
        var model = ToModel(account, "");
        account.TryCount = 0;
        await Repository.UpdateAsync(account);
        return Ok(model);
    }

    /// <summary>
    /// 帳號登出
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult> Logout()
    {
        await AuditEventService.Add(AuditEventType.Logout, "", $"PRAC.{TokenManager.UsingInstitutionNo}.{User.Identity!.Name}");
        return Ok();
    }

    /// <summary>
    /// 變更密碼
    /// </summary>
    /// <returns></returns>
    [DisplayName("變更密碼")]
    [HttpPut]
    public async Task<IActionResult> ChangePassword(ChangePermitModel body)
    {
        var hashPermit = TokenManager.ToHash(body.Old);
        var acc = Repository.DataSet.FirstOrDefault(c => c.No == User.Identity.Name);
        if (acc == null) return Unauthorized("帳號不存在！");
        if (acc.HashPermit != hashPermit)
            return Unauthorized("舊密碼錯誤!");

        //if (!TokenManager.IsPasswordComplex(body.New))
        //    return BadRequest("未符合密碼複雜度要求(12碼以上，大小寫英文、數字、符號)！");

        //var pclogRep = new PwdchangelogRepository(Repository);
        //acc.HashPermit = TokenManager.ToHash(body.New);
        //var pwdlogs = pclogRep.DataSet.Where(c => c.Account == acc.No)
        //    .OrderByDescending(c => c.CreateTime).Take(3).ToList();
        //foreach (var item in pwdlogs)
        //{
        //    if (item.Password == TokenManager.ToHash(body.New))
        //        return BadRequest("新密碼不能與前3次使用過之密碼相同");
        //}
        //var model = new Pwdchangelog
        //{
        //    Account = User.Identity!.Name,
        //    Password = acc.HashPermit,
        //    CreateTime = DateTime.Now,
        //    CreatePractitionerId = User.Identity!.Name
        //};
        //await pclogRep.InsertAsync(model);

        acc.HashPermit = TokenManager.ToHash(body.New);
        acc.LastLoginTime = DateTime.Now;
        acc.PermitChangeTime = DateTime.Now.AddDays(90);
        await Repository.UpdateAsync(acc);
        if (!string.IsNullOrEmpty(TokenManager.UsingInstitutionNo))
            await AuditEventService.Add((AuditEventType) AuditType.ChangePwd, acc, TokenManager.Operationer(User.Identity!.Name));
        return NoContent();
    }

    /// <summary>
    /// 取得帳號身份證字號
    /// </summary>
    /// <param name="user">帳號</param>
    /// <returns></returns>
    [DisplayName("取得帳號身份證字號")]
    [HttpGet]
    [AllowAnonymous]
    public string GetIdentifier(string user)
    {
        var account = Repository.Find(user);
        return account?.Identifier;
    }

    #region 產生base64圖片
    /// <summary>
    /// 產生數字+字母驗證碼
    /// </summary>
    /// <param name="n">驗證碼數</param>
    /// <returns></returns>
    private static string CreateCharCode(int n)
    {
        char[] strChar = { 'a', 'b','c','d','e','f','g','h','i','j','k',/*'l',*/'m',
            'n',/*'o',*/'p','q','r','s','t','u','v','w','x','y','z',/*'0','1',*/'2','3',
            '4','5','6','7','8','9','A','B','C','D','E','F','G','H','I','J','K',
            'L','M','N',/*'O',*/'P','Q','R','S','T','U','V','W','X','Y','Z'};

        string charCode = string.Empty;
        Random random = new Random();
        for (int i = 0; i < n; i++)
        {
            charCode += strChar[random.Next(strChar.Length)];
        }
        return charCode;
    }

    /// <summary>
    /// 獲取驗證碼
    /// </summary>
    /// <param name="charCode">驗證圖字數</param>
    /// <returns></returns>
    private static string CreateCaptchaImage(string charCode)
    {
        int codeW = 100;//寬度
        int codeH = 42;//高度
        int fontSize = 20;//字型大小
                          //初始化驗證碼
                          //顏色列表
        Color[] colors = { Color.Black, Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Brown, Color.DarkBlue };
        //字型列表
        string[] fonts = { "Times New Roman", "Verdana", "Cambria", "Verdana" };
        //建立畫布
        Bitmap bitmap = new Bitmap(codeW, codeH);
        Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);
        Random random = new Random();
        //畫躁線
        for (int i = 0; i < charCode.Length; i++)
        {
            int x1 = random.Next(codeW);
            int y1 = random.Next(codeH);
            int x2 = random.Next(codeW);
            int y2 = random.Next(codeH);
            Color color = colors[random.Next(colors.Length)];
            Pen pen = new Pen(color);
            graphics.DrawLine(pen, x1, y1, x2, y2);
        }
        //畫噪點
        for (int i = 0; i < 100; i++)
        {
            int x = random.Next(codeW);
            int y = random.Next(codeH);
            Color color = colors[random.Next(colors.Length)];
            bitmap.SetPixel(x, y, color);
        }
        //畫驗證碼
        for (int i = 0; i < charCode.Length; i++)
        {
            var fontStr = fonts[random.Next(fonts.Length)];
            var font = new Font(fontStr, fontSize);
            var color = colors[random.Next(colors.Length)];
            graphics.DrawString(charCode[i].ToString(), font, new SolidBrush(color), (float)i * 22 + 5, 6);
        }
        //寫入記憶體流
        try
        {
            var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Jpeg);
            return "data:image/jpeg;base64," + Convert.ToBase64String(stream.ToArray());
        }
        //釋放資源
        finally
        {
            graphics.Dispose();
            bitmap.Dispose();
        }
    }
    #endregion

    /// <summary>
    /// 取得驗證圖(有效時間 3分鐘)
    /// </summary>
    /// <returns></returns>
    [DisplayName("取得驗證圖")]
    [HttpGet]
    [AllowAnonymous]
    public CaptchaModel GetCaptcha()
    {
        var expTime = DateTime.Now.AddMinutes(3).AddSeconds(2).Ticks; //取得亂數

        var validateCode = CreateCharCode(4);

        var hashCode =
            $"{Convert.ToBase64String(Encoding.UTF8.GetBytes($"{validateCode.ToLower()}"))}#{CryptoHelper.ToHash256(validateCode.ToLower(), "DataCom.Captcha")}";
        CaptchaHashCodes[hashCode] = expTime;

        //取得圖片物件
        return new CaptchaModel
        {
            //Code = validateCode.ToLower(),
            HashCode = hashCode,
            Image = CreateCaptchaImage(validateCode)
        };
    }

    private bool CheckCaptcha(string captchaHashCode, bool isRemove = true)
    {
        try
        {
            if (!CaptchaHashCodes.ContainsKey(captchaHashCode))
            {
                return false;
            }
            if (isRemove)
                CaptchaHashCodes.Remove(captchaHashCode);

            var expCaptchaTicks = CaptchaHashCodes.Values.Where(expTime => expTime < DateTime.Now.Ticks).ToList();
            foreach (var expCaptchaTick in expCaptchaTicks)
            {
                var index = CaptchaHashCodes.IndexOfValue(expCaptchaTick);
                if (index > -1) CaptchaHashCodes.RemoveAt(index);
            }

            var captchas = captchaHashCode.Split('#');
            var code = Encoding.UTF8.GetString(Convert.FromBase64String(captchas[0]));
            if (captchas.Length != 2)
            {
                return false;
            }

            var hashCode =
                CryptoHelper.ToHash256(code, "DataCom.Captcha");
            if (hashCode == captchas[1])
            {
                return true;
            }
        }
        catch
        {
            // ignored
        }

        return false;
    }

    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="auditEventService"></param>
    /// <param name="accessor"></param>
    public AuthController(IAuditEventService auditEventService, IHttpContextAccessor accessor) : base(auditEventService, accessor)
    {
    }
}