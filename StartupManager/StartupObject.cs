using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartupManager
{
    public enum StartupType
    {
        ///<summary>
        ///32-bit HKEY_LOCAL_MACHINE
        ///</summary>
        REGISTRY32_LM,
        ///<summary>
        ///32-bit HKEY_CURRENT_USER
        ///</summary>
        REGISTRY32_CU,
        ///<summary>
        ///64-bit HKEY_LOCAL_MACHINE
        ///</summary>
        REGISTRY64_LM,
        ///<summary>
        ///64-bit HKEY_CURRENT_USER
        ///</summary>
        REGISTRY64_CU,
        ///<summary>
        ///Startup folder
        ///</summary>
        FOLDER
    }

    public class StartupObject
    {
        public static int Count;
        private int _id;

        public StartupObject()
        {
            _id = Count;
            Count++;
        }
        
        public int ID
        {
            get
            {
                return _id;
            }
        }

        ///<summary>
        ///Name of the startup object.
        ///</summary>
        public string Name;

        ///<summary>
        ///Path of the startup object to be launched from.
        ///</summary>
        public string Path;

        ///<summary>
        ///The startup object type
        ///</summary>
        public StartupType Type;
    }
}
