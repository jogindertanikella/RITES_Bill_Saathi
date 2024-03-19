using System;
namespace RITES_Bill_Saathi.Models
{
	public class BillDetails
	{
        public string objectId { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string employee_objectId { get; set; }
        public string employeeId { get; set; }
        public string billType { get; set; }
        public string billAmount { get; set; }
        public string fileName { get; set; }
        public string filePath { get; set; }
        public string generatingVisible { get; set; }
        public string billDescription { get; set; }
    }

    public class Results_BillDetails
    {
        public List<BillDetails> results { get; set; }
    }
}

