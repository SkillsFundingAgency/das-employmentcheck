namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class LearnerPayeCheckPriority
    {
        public LearnerPayeCheckPriority() { }

        public LearnerPayeCheckPriority(string payeScheme, int priorityOrder)
        {
            PriorityOrder = priorityOrder;
            PayeScheme = payeScheme;
        }

        public int PriorityOrder { get; set; }

        public string PayeScheme { get; set; }
    }
}