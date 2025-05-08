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
                Tasks = new List<StaffTask>(); // No active tasks when viewing completed only
                CompletedTasks = await _taskService.GetCompletedTasksAsync();
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

            // Handle validation manually since we need to set some properties
            if (string.IsNullOrEmpty(NewTask.Title) || NewTask.DueDate == default)
            {
                TempData["StatusMessage"] = "Error: Please check your input. Title and due date are required.";
                TempData["StatusType"] = "Error";
                return RedirectToPage();
            }

            // Set required properties
            NewTask.CreatedDate = DateTime.Now;
            NewTask.AssignedToId = User.Identity.Name; // Assign to current user
            NewTask.IsComplete = false;
            
            try
            {
                await _taskService.CreateTaskAsync(NewTask);
                TempData["StatusMessage"] = "Task created successfully.";
                TempData["StatusType"] = "Success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = $"Error creating task: {ex.Message}";
                TempData["StatusType"] = "Error";
            }

            return RedirectToPage();
        }

        [BindProperty]
        public StaffTask Task { get; set; }

        public async Task<IActionResult> OnPostEditTaskAsync(int id)
        {
            if (Task == null)
            {
                TempData["StatusMessage"] = "Error: Task details are missing.";
                TempData["StatusType"] = "Error";
                return RedirectToPage();
            }

            // Handle validation manually
            if (string.IsNullOrEmpty(Task.Title) || Task.DueDate == default)
            {
                TempData["StatusMessage"] = "Error: Please check your input. Title and due date are required.";
                TempData["StatusType"] = "Error";
                return RedirectToPage();
            }

            try
            {
                // Get the existing task
                var existingTask = await _taskService.GetTaskByIdAsync(id);
                if (existingTask == null)
                {
                    TempData["StatusMessage"] = "Error: Task not found.";
                    TempData["StatusType"] = "Error";
                    return RedirectToPage();
                }

                // Update only the editable fields
                existingTask.Title = Task.Title;
                existingTask.Description = Task.Description;
                existingTask.Priority = Task.Priority;
                existingTask.DueDate = Task.DueDate;

                await _taskService.UpdateTaskAsync(existingTask);
                TempData["StatusMessage"] = "Task updated successfully.";
                TempData["StatusType"] = "Success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = $"Error updating task: {ex.Message}";
                TempData["StatusType"] = "Error";
            }

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