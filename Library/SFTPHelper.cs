using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Api.Library
{
	/// <summary>
	/// DFtp Class
	/// </summary>
	public class SFTPHelper
	{
		private SftpClient sftp;
		/// <summary>
		/// 連接狀態
		/// </summary>
		public bool Connected
		{
			get
			{
				return this.sftp.IsConnected;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="host">sftp的IP</param>
		/// <param name="port">sftp的端口</param>
		/// <param name="username">sftp的帳戶</param>
		/// <param name="password">sftp的密碼</param>
		public SFTPHelper(string host, string port, string username, string password)
		{
			this.sftp = new SftpClient(host, Int32.Parse(port), username, password);
		}
		/// <summary>
		/// 連接sftp服務器
		/// </summary>
		/// <returns>連接狀態</returns>
		public bool Connect()
		{
			bool result;
			try
			{
				bool flag = !this.Connected;
				if (flag)
				{
					this.sftp.Connect();
				}
				result = true;
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("連接SFTP失敗，原因：{0}", ex.Message));
			}
			return result;
		}
		/// <summary>
		/// 斷開連接
		/// </summary>
		public void Disconnect()
		{
			try
			{
				bool flag = this.sftp != null && this.Connected;
				if (flag)
				{
					this.sftp.Disconnect();
				}
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("斷開SFTP失敗，原因：{0}", ex.Message));
			}
		}
		/// <summary>
		/// 上傳文件
		/// </summary>
		/// <param name="localPath">本地文件路徑</param>
		/// <param name="remotePath">服務器端文件路徑</param>
		public void Put(string localPath, string remotePath)
		{
			try
			{
				using (FileStream fileStream = File.OpenRead(localPath))
				{
					this.Connect();
					this.sftp.UploadFile(fileStream, remotePath, null);
					this.Disconnect();
				}
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("SFTP文件上傳失敗，原因：{0}", ex.Message));
			}
		}
		/// <summary>
		/// 上傳文字字節數據
		/// </summary>
		/// <param name="fileByteArr">文件內容字節</param>
		/// <param name="remotePath">上傳到服務器的路徑</param>
		public void Put(byte[] fileByteArr, string remotePath)
		{
			try
			{
				Stream input = new MemoryStream(fileByteArr);
				this.Connect();
				this.sftp.UploadFile(input, remotePath, null);
				this.Disconnect();
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("SFTP文件上傳失敗，原因：{0}", ex.Message));
			}
		}
		/// <summary>
		/// 將sftp服務器的文件下載本地
		/// </summary>
		/// <param name="remotePath">服務器上的路徑</param>
		/// <param name="localPath">本地的路徑</param>
		public void Get(string remotePath, string localPath)
		{
			try
			{
				this.Connect();
				byte[] bytes = this.sftp.ReadAllBytes(remotePath);
				this.Disconnect();
				File.WriteAllBytes(localPath, bytes);
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("SFTP文件獲取失敗，原因：{0}", ex.Message));
			}
		}

		/// <summary>
		///  刪除ftp服務器上的文件
		/// </summary>
		/// <param name="remoteFile">服務器上的路徑</param>
		public void Delete(string remoteFile)
		{
			try
			{
				this.Connect();
				this.sftp.Delete(remoteFile);
				this.Disconnect();
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("SFTP文件刪除失敗，原因：{0}", ex.Message));
			}
		}

		/// <summary>
		/// 獲取ftp服務器上指定路徑上的指定前綴的文件名列表
		/// </summary>
		/// <param name="remotePath">服務器上的路徑</param>
		/// <param name="fileSuffix">文件名前綴</param>
		/// <returns></returns>
		public List<string> GetFileList(string remotePath, string fileSuffix)
		{
			List<string> result;
			try
			{
				this.Connect();
				IEnumerable<SftpFile> enumerable = (IEnumerable<SftpFile>)this.sftp.ListDirectory(remotePath, null);
				this.Disconnect();
				result = new List<string>();
				foreach (SftpFile current in enumerable)
				{
					string name = current.Name;
					bool flag = name.Length > fileSuffix.Length + 1 && fileSuffix == name.Substring(name.Length - fileSuffix.Length);
					if (flag)
					{
						result.Add(name);
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("SFTP文件列表獲取失敗，原因：{0}", ex.Message));
			}
			return result;
		}
		/// <summary>
		/// ftp服務器端文件移動
		/// </summary>
		/// <param name="oldRemotePath">原來服務器上路徑</param>
		/// <param name="newRemotePath">移動後服務器上新路徑</param>
		public void Move(string oldRemotePath, string newRemotePath)
		{
			try
			{
				this.Connect();
				this.sftp.RenameFile(oldRemotePath, newRemotePath);
				this.Disconnect();
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("SFTP文件移動失敗，原因：{0}", ex.Message));
			}
		}

		#region 創建目錄
		/// <summary>
		/// 循環創建目錄
		/// </summary>
		/// <param name="remotePath">遠程目錄</param>
		public void CreateDirectory(string remotePath)
		{
			try
			{
				string[] paths = remotePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				string curPath = "/";
				for (int i = 0; i < paths.Length; i++)
				{
					curPath += paths[i];
					if (!sftp.Exists(curPath))
					{
						sftp.CreateDirectory(curPath);
					}
					if (i < paths.Length - 1)
					{
						curPath += "/";
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("創建目錄失敗，原因：{0}", ex.Message));
			}
		}
		#endregion

		/// <summary>
		/// 讀取目錄
		/// </summary>
		/// <param name="remotePath">服務器上的路徑</param>
		/// <returns></returns>
		public DateTime GetDirectory(string remotePath)
		{
			DateTime result;
			try
			{
				this.Connect();
				result=this.sftp.GetLastWriteTime(remotePath);
				this.Disconnect();
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("讀取目錄日期，原因：{0}", ex.Message));
			}
			return result;
		}
		/// <summary>
		///  刪除ftp服務器上的目錄
		/// </summary>
		/// <param name="path">服務器上的路徑</param>
		public void DeleteDir(string path)
		{
			foreach (SftpFile file in this.sftp.ListDirectory(path))
			{
				if ((file.Name != ".") && (file.Name != ".."))
				{
					if (file.IsDirectory)
					{
						DeleteDirectory(this.sftp, file.FullName);
					}
					else
					{
						this.sftp.DeleteFile(file.FullName);
					}
				}
			}
			this.sftp.DeleteDirectory(path);
		}

		/// <summary>
		///  刪除ftp服務器上的目錄
		/// </summary>
		/// <param name="client">服務器</param>
		/// <param name="path">服務器上的路徑</param>
		public static void DeleteDirectory(SftpClient client, string path)
		{
			foreach (SftpFile file in client.ListDirectory(path))
			{
				if ((file.Name != ".") && (file.Name != ".."))
				{
					if (file.IsDirectory)
					{
						DeleteDirectory(client, file.FullName);
					}
					else
					{
						client.DeleteFile(file.FullName);
					}
				}
			}

			client.DeleteDirectory(path);
		}
	}
}
