using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmploymentCheck.Functions.Data
{
    public partial class EmployerIncentivesDbContext : DbContext
    {
        public EmployerIncentivesDbContext()
        {
        }

        public EmployerIncentivesDbContext(DbContextOptions<EmployerIncentivesDbContext> options) : base(options)
        {
        }

        public virtual DbSet<ApprenticeshipBreakInLearning> ApprenticeshipBreakInLearnings { get; set; }
        public virtual DbSet<ApprenticeshipDaysInLearning> ApprenticeshipDaysInLearnings { get; set; }
        public virtual DbSet<ApprenticeshipIncentive> ApprenticeshipIncentives { get; set; }
        public virtual DbSet<ClawbackPayment> ClawbackPayments { get; set; }
        public virtual DbSet<Learner> Learners { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<PendingPayment> PendingPayments { get; set; }
        public virtual DbSet<PendingPaymentValidationResult> PendingPaymentValidationResults { get; set; }
    }
}
