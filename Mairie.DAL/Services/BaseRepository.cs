using Mairie.DAL.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mairie.DAL.Services
{
    public abstract class BaseRepository
    {
        protected DatabaseConfiguration _dbConfig;

        public BaseRepository(DatabaseConfiguration dbConfig)
        {
            _dbConfig = dbConfig ?? throw new ArgumentNullException(nameof(dbConfig));
        }
    }
}
