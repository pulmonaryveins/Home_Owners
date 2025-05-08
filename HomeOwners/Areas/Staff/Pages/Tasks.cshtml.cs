// HomeOwners/Areas/Staff/Pages/Tasks.cshtml.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Staff.Pages
{
    [Authorize(Policy = "RequireStaffRole")]
    public class TasksModel : PageModel
    {
        private readonly TaskService _taskService;

        public TasksModel(TaskService taskService)
        {
            _taskService = taskService;
        }

        public List<StaffTask> Tasks { get; set; } = new List<StaffTask>();
        public List<StaffTask> CompletedTasks { get; set; } = new List<StaffTask>();

        [BindProperty]
        public StaffTask NewTask { get; set; }

        public string StatusMessage { get; set; }
        public string StatusType { get; set; }

        public async Task OnGetAsync(string filter = "all")
        {
            if (filter == "completed")
            {
                Tasks = await _taskService.GetCompletedTasksAsync();
                CompletedTasks = new List<StaffTask>();
            }
            else if (filter == "active")
            {
                Tasks = await _taskService.GetIncompleteTasksAsync();
                CompletedTasks = new List<StaffTask>();
            }
            else
            {
                Tasks = await _taskService.GetIncompleteTasksAsync();
                CompletedTasks = await _taskService.GetCompletedTasksAsync();
            }

            StatusMessage = TempData["StatusMessage"]?.ToString();
            StatusType = TempData["StatusType"]?.ToString();
        }

        public async Task<IActionResult> OnPostCreateTask()
        {
            if (NewTask == null)
            {
                TempData["StatusMessage"] = "Error: Task details are missing.";
                TempData["StatusType"] = "Error";
                return RedirectToPage();
            }

            if (!ModelState.IsValid)
            {
                TempData["StatusMessage"] = "Error: Please check your input.";
                TempData["StatusType"] = "Error";
                return RedirectToPage();
            }

            NewTask.CreatedDate = DateTime.Now;
            await _taskService.CreateTaskAsync(NewTask);

            TempData["StatusMessage"] = "Task created successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostToggleCompletionAsync(int id)
        {
            await _taskService.ToggleTaskCompletionAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _taskService.DeleteTaskAsync(id);

            TempData["StatusMessage"] = "Task deleted successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }
    }
}