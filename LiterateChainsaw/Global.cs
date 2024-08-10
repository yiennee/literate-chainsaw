using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnimatedDisco;

namespace LiterateChainsaw
{
    public static class Global
    {
        public static int SelectedCoordX;
        public static int SelectedCoordY;
        private static string _isTCPIPReplyToolMsg;

        public static UserControl uc = new UserControl();


        static Global()
        {
            try
            {
                _isTCPIPReplyToolMsg = "";

            }
            catch (Exception ee)
            {

            }
        }

        public static string IsTCPIPReplyToolMsg
        {
            get
            {
                return _isTCPIPReplyToolMsg;
            }
            set
            {
                _isTCPIPReplyToolMsg = value;
            }
        }
    }
}
