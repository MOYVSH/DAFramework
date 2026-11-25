using DAPool;
using DAFramework.Buff;
using DAFramework.Entity;
using DAFramework.ReferencePool;
using DAFramework.Procedure;

namespace DAFramework
{
    public static class DA
    {
        /* ------------------------核心模块------------------------ */
        private static ReferenceManager _reference;
        private static ConfigManager _config;
        private static EntityManager _entity;
        private static UIManager _ui;
        private static BuffManager _buff;
        private static ProcedureManager _procedure;
        private static GameManager _gm;
        private static ObjectPoolManager _pool;
        /* ------------------------可选模块------------------------ */
        public static ReferenceManager Reference
        {
            get
            {
                if (_reference == null)
                    _reference = ModuleCenter.CreateModule<ReferenceManager>();
                return _reference;
            }
            set
            {
                if (_reference == null)
                    _reference = value;
            }
        }

        public static ConfigManager Config
        {
            get
            {
                if (_config == null)
                    _config = ModuleCenter.CreateModule<ConfigManager>();
                return _config;
            }
            set
            {
                if (_config == null)
                    _config = value;
            }
        }

        public static EntityManager Entity
        {
            get
            {
                if (_entity == null)
                    _entity = ModuleCenter.CreateModule<EntityManager>();
                return _entity;
            }
            set
            {
                if (_entity == null)
                    _entity = value;
            }
        }

        public static UIManager UI
        {
            get
            {
                if (_ui == null)
                    _ui = ModuleCenter.CreateModule<UIManager>();
                return _ui;
            }
            set
            {
                if (_ui == null)
                    _ui = value;
            }
        }

        public static BuffManager Buff
        {
            get
            {
                if (_buff == null)
                    _buff = ModuleCenter.CreateModule<BuffManager>();
                return _buff;
            }
            set
            {
                if (_buff == null)
                    _buff = value;
            }
        }
        
        public static ProcedureManager Procedure
        {
            get
            {
                if (_procedure == null)
                    _procedure = ModuleCenter.CreateModule<ProcedureManager>();
                return _procedure;
            }
            set
            {
                if (_procedure == null)
                    _procedure = value;
            }
        }

        public static GameManager GM {
            get
            {
                if (_gm == null)
                    _gm = ModuleCenter.CreateModule<GameManager>();
                return _gm;
            }
            set
            {
                if (_gm == null)
                    _gm = value;
            }
        }

        public static ObjectPoolManager Pool {
            get
            {
                if (_pool == null)
                    _pool = ModuleCenter.CreateModule<ObjectPoolManager>();
                return _pool;
            }
            set
            {
                if (_pool == null)
                    _pool = value;
            }
        }
    }
}
