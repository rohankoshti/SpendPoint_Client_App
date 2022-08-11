using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpendPoint
{
    public class Memory
    {
        public string Client;
        public string Supervisor;
        public string Representative;
        public string JobNumber;

        public static Memory FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            Memory memory = new Memory
            {
                Client = values[0],
                Representative = values[1],
                Supervisor = values[2],
                JobNumber = values[3]
            };
            return memory;
        }

        public List<Memory> GetMemoryRecords(string file, int count = 1000)
        {
            List<Memory> records = File.ReadAllLines(file)
                                           .Skip(1)
                                           .Select(v => Memory.FromCsv(v))
                                           .ToList();
            records = records.Skip(Math.Max(0, records.Count() - count)).ToList();
            return records;
        }

        public int AppendToMemoryFile(string file, OutputSummary summary, int currentIndex, List<Memory> currentMemories)
        {
            if (!File.Exists(file))
            {
                string header = "Client,Representative,Supervisor,JobNumber" + Environment.NewLine;
                File.AppendAllText(file, header);
            }

            if (currentMemories.Any() && currentMemories.Count == currentIndex)
            {
                currentIndex--;
            }

            if (currentIndex <= 0 || !currentMemories[currentIndex].Client.Equals(summary.ClientName) ||
               !currentMemories[currentIndex].Representative.Equals(summary.RepresentativeName) ||
                !currentMemories[currentIndex].Supervisor.Equals(summary.SupervisorName) ||
                !currentMemories[currentIndex].JobNumber.Equals(summary.JobNumber))
            {
                // new record is inserted
                var data = summary.ClientName.RemoveCommas() + "," + summary.RepresentativeName.RemoveCommas() + "," + summary.SupervisorName.RemoveCommas() + ","
                + summary.JobNumber.RemoveCommas() + Environment.NewLine;
                File.AppendAllText(file, data);
                currentIndex--;
            }
            return currentIndex;
        }

        public void CreateMemoryFile(string file)
        {
            if (!File.Exists(file))
            {
                string header = "Client,Representative,Supervisor,JobNumber" + Environment.NewLine;
                File.WriteAllText(file, header);
            }
        }
    }
}
