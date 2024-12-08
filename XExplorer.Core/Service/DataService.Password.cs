using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using XExplorer.Core.Context;
using XExplorer.Core.Modes;

namespace XExplorer.Core.Service;

/// <summary>
/// The DataService class is responsible for managing the data operations
/// within the application. It provides methods to interact with the data
/// layer, allowing for the retrieval, modification, and processing of data
/// used by the application.
/// </summary>
public partial class DataService
{
     /// <summary>
     /// The PasswordService class provides functionality for managing passwords
     /// within the application. It offers methods to perform operations such as
     /// adding new passwords and retrieving all existing passwords from the database.
     /// </summary>
     public partial class PasswordService
     {
          /// <summary>
          /// Provides functionality for managing passwords within the application.
          /// </summary>
          public PasswordService(SQLiteContext context)
          {
               this.dataContext = context;
          }
          
          /// <summary>
          /// 数据上下文
          /// </summary>
          private SQLiteContext dataContext;
    
          /// <summary>
          /// 异步添加一个新的密码到数据库中。
          /// </summary>
          /// <param name="pwd">要添加的密码。</param>
          /// <returns>一个表示异步操作的任务。</returns>
          public async Task AddAsync(string pwd)
          {
               if (string.IsNullOrWhiteSpace(pwd))
                    return;
		
               if (!this.dataContext.Passwords.Any(m => m.Pwd == pwd))
               {
                    this.dataContext.Passwords.Add(new Password { Pwd = pwd, Id = this.dataContext.IdGenerator.CreateId() });
                    await this.dataContext.SaveChangesAsync();
               }
          }

          /// <summary>
          /// Asynchronously retrieves a list of all passwords from the database.
          /// </summary>
          /// <returns>A task representing the asynchronous operation, with a list of passwords as its result.</returns>
          public async Task<List<Password>> GetAsync() => await this.dataContext.Passwords.ToListAsync();
     }
}