using System;
using System.Linq;

namespace FantasyFootballRobot.Core.Contracts
{
   public interface IDataStorageSession : IDisposable
   {
      IQueryable<T> GetAll<T>();
      T GetById<T>(string id);
      void CreateOrUpdate<T>(T entity);
      void Delete<T>(T entity);
      void SaveChanges();
   }
}