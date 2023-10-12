using System;
using System.Collections.Generic;
using System.Text;

namespace domain.repository
{
    public interface ISignService
    {
        bool AddSign(String Sign, DateTime ServerTime, DateTime ClientTime, String Controller, String Action);
    }
}
