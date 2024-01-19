using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMSS.Models;
using System;


namespace SMSS.Data
{
    public class ApplicationDbContext : IdentityDbContext<RegisteredUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {




            base.OnModelCreating(builder);


            builder.Entity<RegisteredUser>(b =>
            {
                b.ToTable("RegisteredUsers");
            });



            builder.Entity<IdentityUserClaim<int>>(b =>
            {
                b.ToTable("UserClaims");
            });

            builder.Entity<IdentityUserLogin<int>>(b =>
            {
                b.ToTable("UserLogins");
            });

            builder.Entity<IdentityUserToken<int>>(b =>
            {
                b.ToTable("UserTokens");
            });

            builder.Entity<IdentityRole<int>>(b =>
            {
                b.ToTable("Roles");
            });

            builder.Entity<IdentityRoleClaim<int>>(b =>
            {
                b.ToTable("RoleClaims");
            });

            builder.Entity<IdentityUserRole<int>>(b =>
            {
                b.ToTable("UserRoles");
            });

            builder.Entity<UserSector>(b =>
            {
                b.ToTable("UserSectors");
            });



            //builder.Entity<EmailSubscription>().HasIndex(b => b.Email).IsUnique();

            //builder.Entity<UserSector>()
            //    .HasOne(ac => ac.registeredUser)
            //    .WithMany(au => au.UserSectors)
            //    .HasForeignKey(ac => ac.RegisteredUserId);

            //builder.Entity<UserSector>()
            //    .HasOne(au => au.Sector)
            //    .WithMany()
            //    .HasForeignKey(se => se.SectorId);

            builder.Entity<ApplicantProfile>()
                .Property(ap => ap.RegistrationDate)
                .HasDefaultValueSql("getdate()");

            builder.Entity<CompanyProfile>()
                .Property(ap => ap.RegistrationDate)
                .HasDefaultValueSql("getdate()");

            builder.Entity<CompanyLocation>()
                .HasOne(cl => cl.Country)
                .WithMany(co => co.CompanyLocations)
                .OnDelete(DeleteBehavior.NoAction);


            //builder.Entity<UnsubscribeUserReason>()
            //    .HasKey(c => new { c.UnsubscribeUserId, c.UnsubscribeReasonId });

            //builder.Entity<UnsubscribeUserReason>()
            //    .HasOne(ur => ur.UnsubscribeUser)
            //    .WithMany(c => c.UnsubscribeUserReasons)
            //    .HasForeignKey(ur => ur.UnsubscribeUserId);

            //builder.Entity<UnsubscribeUserReason>()
            //    .HasOne(ur => ur.UnsubscribeReason)
            //    .WithMany(unr => unr.UnsubscribeUserReasons)
            //    .HasForeignKey(ur => ur.UnsubscribeReasonId);

            

            builder.Entity<CompanyLocation>()
                .HasOne(cl => cl.City)
                .WithMany(ci => ci.CompanyLocations)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CompanyLocation>()
                .HasOne(cl => cl.Province)
                .WithMany(pr => pr.CompanyLocations)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CompanyJob>()
               .HasOne(cj => cj.Country)
               .WithMany(co => co.CompanyJobs)
               .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CompanyJob>()
                .HasOne(cj => cj.City)
                .WithMany(ci => ci.CompanyJobs)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CompanyJob>()
                .HasOne(cj => cj.Province)
                .WithMany(pr => pr.CompanyJobs)
                .OnDelete(DeleteBehavior.NoAction);



            builder.Entity<ApplicantJobApplication>()
                .HasOne(au => au.CompanyJob)
                .WithMany(ap => ap.ApplicantJobApplications)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ApplicantJobApplication>()
              .HasOne(au => au.ApplicantProfile)
              .WithMany(ap => ap.ApplicantJobApplications)
              .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ApplicantJobApplication>()
                   .Property(b => b.ApplicationStatus)
                   .HasDefaultValue(ApplicationStatus.New);

            builder
                 .Entity<SectorJobCount>(
            eb =>
            {
                eb.HasNoKey();
                eb.ToView("V_Sector_Job_Count");
                eb.Property(v => v.Id).HasColumnName("ID");
                eb.Property(v => v.SectorName).HasColumnName("Sector_Name");
                eb.Property(v => v.JobCount).HasColumnName("Job_Count");
            });

            builder.Entity<Sector>().HasData(
                new Sector { Id = 1, SectorName = "Application Developer" },
                new Sector { Id = 2, SectorName = "Application Testing and Quality Assurance" },
                new Sector { Id = 3, SectorName = "Applications Architect" },
                new Sector { Id = 4, SectorName = "Automation Tester" },
                new Sector { Id = 5, SectorName = "Azure Developer" },

                new Sector { Id = 6, SectorName = "Big Data Developer" },
                new Sector { Id = 7, SectorName = "Business Analyst" },
                new Sector { Id = 8, SectorName = "Business Integration Specialist" },
                new Sector { Id = 9, SectorName = "Business Intelligence Analyst" },

                new Sector { Id = 10, SectorName = "Change Management Consultant" },
                new Sector { Id = 11, SectorName = "Cyber Security Designer" },

                new Sector { Id = 12, SectorName = "Data Conversion Specialist" },
                new Sector { Id = 13, SectorName = "Database Administrator" },
                new Sector { Id = 14, SectorName = "Database Administrator(Azure Cloud Exp)" },
                new Sector { Id = 15, SectorName = "Desktop Specialist" },
                new Sector { Id = 16, SectorName = "DevOps Engineer" },
                new Sector { Id = 17, SectorName = "Dot Net Programmer" },

                new Sector { Id = 18, SectorName = "EDL Analyst" },
                new Sector { Id = 19, SectorName = "Enterprise Architect" },
                new Sector { Id = 20, SectorName = "ERP Technical Analyst" },
                new Sector { Id = 21, SectorName = "ETL Developer" },
                new Sector { Id = 22, SectorName = "Event Coordinator" },

                new Sector { Id = 23, SectorName = "Field Test Engineer" },
                new Sector { Id = 24, SectorName = "Front End Developer" },
                new Sector { Id = 25, SectorName = "Full Stack Developer" },

                new Sector { Id = 26, SectorName = "Helpdesk Analyst" },


                new Sector { Id = 27, SectorName = "IBM Integration Designer" },
                new Sector { Id = 28, SectorName = "Informatica Developer" },
                new Sector { Id = 29, SectorName = "Information Architect" },
                new Sector { Id = 30, SectorName = "Infrastructure Integration Specialist" },
                new Sector { Id = 31, SectorName = "Integration Architect" },
                new Sector { Id = 32, SectorName = "IT Technical Support Specialist" },
                new Sector { Id = 33, SectorName = "IVR Developer" },

                new Sector { Id = 34, SectorName = "Java Developer" },

                new Sector { Id = 35, SectorName = "Legal Assistant" },

                new Sector { Id = 36, SectorName = "Mainframe Tester - Developer" },
                new Sector { Id = 37, SectorName = "Management Consultant" },
                new Sector { Id = 38, SectorName = "Middleware Specialist" },

                new Sector { Id = 39, SectorName = "Network Engineer" },

                new Sector { Id = 40, SectorName = "OPS Docs – Architect" },
                new Sector { Id = 41, SectorName = "Oracle Database Administrator" },
                new Sector { Id = 42, SectorName = "Oracle EBS Functional" },
                new Sector { Id = 43, SectorName = "Oracle EBS Project Manager" },
                new Sector { Id = 44, SectorName = "Oracle EBS Supply Chain Functional Lead" },
                new Sector { Id = 45, SectorName = "Oracle EBS Technical " },
                new Sector { Id = 46, SectorName = "Oracle EDI Consultant" },
                new Sector { Id = 47, SectorName = "Oracle SCM Techno Functional" },
                new Sector { Id = 48, SectorName = "Organizational Analyst" },

                new Sector { Id = 49, SectorName = "Portfolio Manager" },
                new Sector { Id = 50, SectorName = "Power BI Developer" },
                new Sector { Id = 51, SectorName = "Privacy Impact Analyst" },
                new Sector { Id = 52, SectorName = "Procurement Specialist" },
                new Sector { Id = 53, SectorName = "Program Manager" },
                new Sector { Id = 54, SectorName = "Programmer Analyst" },
                new Sector { Id = 55, SectorName = "Programmer Developer" },
                new Sector { Id = 56, SectorName = "Project Manager - Leader" },

                new Sector { Id = 57, SectorName = "Rave Medidata Business Analysts and Rave Medidata Project Manager" },
                new Sector { Id = 58, SectorName = "Receptionist" },
                new Sector { Id = 59, SectorName = "Revenue Assurance Analyst" },
                new Sector { Id = 60, SectorName = "Salesforce Architect" },

                new Sector { Id = 61, SectorName = "Salesforce Developer" },
                new Sector { Id = 62, SectorName = "SAP Project Manager" },
                new Sector { Id = 63, SectorName = "SAS Developer" },
                new Sector { Id = 64, SectorName = "Scrum Master" },
                new Sector { Id = 65, SectorName = "Security Architect" },
                new Sector { Id = 66, SectorName = "Selenium Automation Tester" },
                new Sector { Id = 67, SectorName = "Senior Business Analyst" },
                new Sector { Id = 68, SectorName = "Senior Cloud Developer - AWS" },
                new Sector { Id = 69, SectorName = "Senior Consultant - Digital Marketing" },
                new Sector { Id = 70, SectorName = "Senior Data Modeler - Data Analyst" },

                new Sector { Id = 71, SectorName = "Senior Project Manager" },
                new Sector { Id = 72, SectorName = "Server Analyst" },
                new Sector { Id = 73, SectorName = "Server Developer" },
                new Sector { Id = 74, SectorName = "ServiceNow BA" },
                new Sector { Id = 75, SectorName = "ServiceNow Developer" },
                new Sector { Id = 76, SectorName = "SharePoint Specialist" },
                new Sector { Id = 77, SectorName = "Siebel Consultant" },
                new Sector { Id = 78, SectorName = "Software Developer" },
                new Sector { Id = 79, SectorName = "Solution Architect" },
                new Sector { Id = 80, SectorName = "Solutions Designer" },
                new Sector { Id = 81, SectorName = "Splunk Developer" },
                new Sector { Id = 82, SectorName = "Sr.Data Modeler - Data Analyst" },
                new Sector { Id = 83, SectorName = "SQL Database Administrator" },
                new Sector { Id = 84, SectorName = "System Administrator" },
                new Sector { Id = 85, SectorName = "Systems Analyst" },

                new Sector { Id = 86, SectorName = "Task Based - IT Consultant" },
                new Sector { Id = 87, SectorName = "Technical Analyst" },
                new Sector { Id = 88, SectorName = "Technical Architect" },
                new Sector { Id = 89, SectorName = "Technical Business Analyst" },
                new Sector { Id = 90, SectorName = "Technical Lead - Senior Software Developer" },
                new Sector { Id = 91, SectorName = "Technical Specialist" },
                new Sector { Id = 92, SectorName = "Technology Architect" },
                new Sector { Id = 93, SectorName = "Training Specialist" },

                new Sector { Id = 94, SectorName = "Web Developer" },
                new Sector { Id = 95, SectorName = "Web Service Analyst" },
                new Sector { Id = 96, SectorName = "Web Specialist" },
                new Sector { Id = 97, SectorName = "Wi-Fi Expert" },
                new Sector { Id = 98, SectorName = "UX designer" }

                );

            builder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int> { Id = 1, Name = "Admin", NormalizedName = "admin" },
                new IdentityRole<int> { Id = 2, Name = "User", NormalizedName = "user" },
                new IdentityRole<int> { Id = 3, Name = "Recruiter", NormalizedName = "recruiter" }
                //new IdentityRole<int> { Id = 4, Name = "SuperAdmin", NormalizedName = "superadmin" }
                );

            builder.Entity<Country>().HasData(
                new Country { Id = 1, Name = "Canada" },
                new Country { Id = 2, Name = "USA" },
                new Country { Id = 3, Name = "India" }
                );
            



        }
        public DbSet<RegisteredUser> RegisteredUsers { get; set; }
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<UserSector> UserSectors { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<ApplicantProfile> ApplicantProfiles { get; set; }
        public DbSet<CompanyProfile> CompanyProfiles { get; set; }
        public DbSet<ApplicantEducation> ApplicantEducations { get; set; }

        public DbSet<ApplicantSkill> ApplicantSkills { get; set; }

        //public DbSet<CandidateName> CandidateNames { get; set; }
        //public DbSet<CandidateCorporation> CandidateCorporations { get; set; }
        //public DbSet<ClientName> ClientNames { get; set; }

        public DbSet<ApplicantTestimonial> ApplicantTestimonials { get; set; }

        public DbSet<CarouselSliderImage> CarouselSliderImages { get; set; }

        public DbSet<ApplicantWorkHistory> ApplicantWorkHistories { get; set; }
        public DbSet<ApplicantJobApplication> ApplicantJobApplications { get; set; }
        public DbSet<CompanyLocation> CompanyLocations { get; set; }
        public DbSet<CompanyJob> CompanyJobs { get; set; }
        public DbSet<CompanyJobSector> CompanyJobSectors { get; set; }
        public DbSet<SectorJobCount> SectorJobCounts { get; set; }

        public DbSet<ProvinceDemoFileAttachment> ProvinceDemoFileAttachments { get; set; }


        //public DbSet<UnsubscribeUserEmailRequest> UnsubscribeUserEmailRequests { get; set; }

        //public DbSet<UnsubscribeUserEmail> UnsubscribeUserEmails { get; set; }     
        

        public DbSet<JobMode> JobModes { get; set; }

        public DbSet<UnsubscribeUser> UnsubscribeUsers { get; set; }

        public DbSet<HiringClientLogo> HiringClientLogos { get; set; }

        public DbSet<ContactusModel> ContactusModels { get; set; }

        public DbSet<EnrolledCandidatesCount> EnrolledCandidatesCounts { get; set; }


        public static implicit operator ControllerContext(ApplicationDbContext v)
        {
            throw new NotImplementedException();
        }
    }


}
