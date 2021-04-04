namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;

    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using TeisterMask.Data.Models;
    using TeisterMask.DataProcessor.ImportDto;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            var output = new StringBuilder();
            var xmlSerializer = new XmlSerializer(
                typeof(ProjectXmlInputModel[]),
                new XmlRootAttribute("Projects"));

            var projects     =
                (ProjectXmlInputModel[])xmlSerializer.Deserialize(
                    new StringReader(xmlString));

            foreach (var project in projects)
            {
                if (!IsValid(project))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                bool parsedDate = DateTime.TryParseExact(
                    project.OpenDate, "dd/MM/yyyy HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);
                if (!parsedDate)
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(project.DueDate))
                {
                    bool parsedDueDate = DateTime.TryParseExact(
                            project.DueDate, "dd/MM/yyyy HH:mm",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDate);
                    if (!parsedDueDate)
                    {
                        output.AppendLine(ErrorMessage);
                        continue;
                    }
                }

                var projetObj = AutoMapper.Mapper.Map<Project>(project);
                Console.WriteLine();
            }

            return null;
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            var output = new StringBuilder();
            var employees = JsonConvert.DeserializeObject<IEnumerable<EmployeeJsonInputModel>>(jsonString).ToList();

            foreach (var employee in employees)
            {
                if (!IsValid(employee) || !employee.EmployeeTasks.All(IsValid))
                {
                    output.AppendLine(ErrorMessage);
                    continue;
                }

                var employeeObj = AutoMapper.Mapper.Map<Employee>(employee);

                Console.WriteLine();
            }
            return null;
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}