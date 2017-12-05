using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcs
{
    public interface IPCS
    {
        void LocalState(string pid, string round_id);
        void InjectDelay(string src_pid, string dst_pid);
        void Unfreeze(string pid);
        void Freeze(string pid);
        void GlobalStatus();
        void Crash(string pid);
        void StartServer(string pid, string server_url, string msec, string num_players);
        void StartClient(string pid, string client_url, string msec, string num_players, string file_name, string server_url);
        void Print(string str);
    }
}
