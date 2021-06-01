using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AutomatedInputHandler.Helper;

namespace AutomatedInputHandler
{
    public partial class Form1 : Form
    {
        DataConfig CurrentDataConfig;

        private void BuildDataConfig()
        {
            CurrentDataConfig = new DataConfig();

            List<Company> m_Companies = new List<Company>();

            int m_Index;

            foreach (TreeNode company in tvData.Nodes[0].Nodes)
            {
                Company m_Company = new Company();

                string m_CompanyName = company.Text.Substring(company.Text.IndexOf("Company: ") + 9);
                m_CompanyName = m_CompanyName.Substring(0, m_CompanyName.IndexOf(";"));

                m_Company.Identifier = m_CompanyName;

                if (Int32.TryParse(company.Text.Substring(company.Text.IndexOf("Index: ") + 7), out m_Index))
                    m_Company.Index = m_Index;
                else
                    throw new Exception(string.Format("Index of company '{0}' is not integer!", company.Text));                

                foreach (TreeNode year in company.Nodes)
                {
                    if (m_Company.Years == null)
                        m_Company.Years = new List<Year>();

                    Year m_Year = new Year();

                    string m_YearText = year.Text.Substring(year.Text.IndexOf("Year: ") + 6);
                    m_YearText = m_YearText.Substring(0, m_YearText.IndexOf(";"));

                    m_Year.year = m_YearText;

                    if (Int32.TryParse(year.Text.Substring(year.Text.IndexOf("Index: ") + 7), out m_Index))
                        m_Year.Index = m_Index;
                    else
                        throw new Exception(
                            string.Format("Index of '{0}' company's year '{1}' is not integer!",
                            m_CompanyName, m_YearText));                    

                    foreach (TreeNode period in year.Nodes)
                    {
                        if(m_Year.Periods == null)
                            m_Year.Periods = new List<Period>();

                        Period m_Period = new Period();

                        string m_PeriodText = period.Text.Substring(period.Text.IndexOf("Period: ") + 8);
                        m_PeriodText = m_PeriodText.Substring(0, m_PeriodText.IndexOf(";"));

                        m_Period.period = m_PeriodText;

                        if (Int32.TryParse(period.Text.Substring(period.Text.IndexOf("Index: ") + 7), out m_Index))
                            m_Period.Index = m_Index;
                        else
                            throw new Exception(string.Format(
                                "Index of '{0}' company's '{1}' year's period '{2}' is not integer!",
                                m_CompanyName, m_YearText, m_PeriodText));

                        m_Year.Periods.Add(m_Period);
                        m_Year.AllPeriodsCount++;
                    }

                    m_Company.Years.Add(m_Year);
                    m_Company.AllYearsCount++;
                }

                m_Companies.Add(m_Company);
            }

            CurrentDataConfig.Companies = m_Companies;
            CurrentDataConfig.AllCompaniesCount = m_Companies.Count;
        }
    }
}
