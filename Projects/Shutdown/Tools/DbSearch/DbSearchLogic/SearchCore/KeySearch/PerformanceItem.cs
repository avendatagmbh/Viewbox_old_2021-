using System;
using System.Management;

namespace DbSearchLogic.SearchCore.KeySearch
{
    /// <summary>
    /// Represents a momentary performance
    /// </summary>
    public class PerformanceItem{

        #region [ Public fields ]

        /// <summary>
        /// Get the CPU usage
        /// </summary>
        public string CpuUsage { get { return GetProcessorUsage().ToString(); } }

        /// <summary>
        /// Get the free RAM in MB
        /// </summary>
        public string RamFree {
            get {
                //Set the object
                performanceCounter.CategoryName = "Memory";
                performanceCounter.CounterName = "Available MBytes";
                performanceCounter.InstanceName = "";
                return performanceCounter.NextValue() + "MB";
            }
        }

        /// <summary>
        /// Get the allocated RAM in MB
        /// </summary>
        public string RamAllocated {
            get {
                //Set the object
                performanceCounter.CategoryName = "Process";
                performanceCounter.CounterName = "Working Set";
                performanceCounter.InstanceName = "_Total";
                return Math.Round((double) (performanceCounter.NextValue())/(1024*1024), 0) + "MB";
            }
        }

        #endregion [ Public fields ]

        #region [ Private variables ]
        
        /// <summary>
        /// The performance counter object
        /// </summary>
        private System.Diagnostics.PerformanceCounter performanceCounter;

        #endregion [ Private variables ]

        #region [ Constructor ]

        /// <summary>
        /// Constructor
        /// </summary>
        public PerformanceItem(){
            performanceCounter = new System.Diagnostics.PerformanceCounter();
            performanceCounter.ReadOnly = true;
        }

        #endregion [ Constructor ]

        #region [ Private Methods ]

        /// <summary>
        /// Get the CPU usage (using WMI)
        /// </summary>
        /// <returns>The percentage of CPU using</returns>
        private int GetProcessorUsage(){
            ObjectQuery WmiCpus = new WqlObjectQuery("SELECT * FROM Win32_Processor");
            using (ManagementObjectSearcher Cpus = new ManagementObjectSearcher(WmiCpus)) {
                try {
                    int coreCount = 0;
                    int totusage = 0;
                    foreach (ManagementObject cpu in Cpus.Get()) {
                        coreCount += 1;
                        totusage += Convert.ToInt32(cpu["LoadPercentage"]);
                    }
                    if (coreCount > 1) {
                        double ActUtiFloat = totusage/coreCount;
                        return Convert.ToInt32(Math.Round(ActUtiFloat));
                    } else {
                        return totusage;
                    }
                } catch (Exception ex) {
                    throw ex;
                }
            }
        }

        #endregion [ Private Methods ]
    }
}
