using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moniverse.Contract
{
    public interface IRetentionRow
    {

        DateTime date { get; set; }
        int installsOnThisDay { get; set; }
        float[] days { get; set; }

        void SetDayPercent(int i, float percent);
        int GetDayNRetentionCount(DateTime today, int n);
        int GetDayNInstallsCount(DateTime today, int n);
    }

    public interface IGeneralRetentionCalculator
    {

        DateTime date { get; set; }
        int installsOnThisDay { get; set; }
        float[] days { get; set; }

        void SetDayPercent(int i, float percent);
        int GetDayNRetentionCount(DateTime today, int n);
        int GetDayNInstallsCount(DateTime today, int n);
    }
    public interface IBucketedRetentionCalculator
    {
        

    }
}
