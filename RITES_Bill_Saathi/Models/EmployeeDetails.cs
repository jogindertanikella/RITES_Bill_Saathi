using System;
namespace RITES_Bill_Saathi.Models
{
    public class EmployeeDetails
    {
        public string objectId { get; set; }
        public int employee_serialNumebr { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string employeeId { get; set; }
        public string employeeName { get; set; }
        public int employeeBills { get; set; }
    }


    public class Results_EmployeeDetails
    {
        public List<EmployeeDetails> results { get; set; }
    }

}

