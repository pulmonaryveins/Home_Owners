using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeOwners.Services
{
    public class TaskService
    {
        private readonly HomeDbContext _context;

        public TaskService(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<List<StaffTask>> GetAllTasksAsync()
        {
            return await _context.StaffTasks
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<List<StaffTask>> GetCompletedTasksAsync()
        {
            return await _context.StaffTasks
                .Where(t => t.IsComplete)
                .OrderByDescending(t => t.CompletedDate)
                .ToListAsync();
        }

        public async Task<List<StaffTask>> GetIncompleteTasksAsync()
        {
            return await _context.StaffTasks
                .Where(t => !t.IsComplete)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<StaffTask> GetTaskByIdAsync(int id)
        {
            return await _context.StaffTasks.FindAsync(id);
        }

        public async Task<StaffTask> CreateTaskAsync(StaffTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task), "Task cannot be null.");
            }

            if (string.IsNullOrEmpty(task.Title))
            {
                throw new ArgumentException("Task title is required.", nameof(task.Title));
            }

            _context.StaffTasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }


        public async Task<StaffTask> UpdateTaskAsync(StaffTask task)
        {
            _context.StaffTasks.Update(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.StaffTasks.FindAsync(id);
            if (task == null)
                return false;

            _context.StaffTasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleTaskCompletionAsync(int id)
        {
            var task = await _context.StaffTasks.FindAsync(id);
            if (task == null)
                return false;

            task.IsComplete = !task.IsComplete;
            task.CompletedDate = task.IsComplete ? DateTime.Now : null;

            _context.StaffTasks.Update(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<StaffTask>> GetUpcomingTasksAsync(int days = 7)
        {
            var cutoffDate = DateTime.Now.AddDays(days);
            return await _context.StaffTasks
                .Where(t => !t.IsComplete && t.DueDate <= cutoffDate)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToListAsync();
        }
    }
}