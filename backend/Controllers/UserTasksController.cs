using Microsoft.AspNetCore.Mvc;
using task_backend.Models;
using MySql.Data.MySqlClient;
using task_backend.Data;
using Microsoft.AspNetCore.Cors;

namespace task_backend.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    [EnableCors("AllowLocalhost")]
    public class UserTasksController : ControllerBase
    {
        private readonly DatabaseContext _db;

        public UserTasksController(DatabaseContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get a list of tasks for the authenticated user.
        /// </summary>
        /// <returns>Returns a list of user tasks.</returns>
        /// <response code="200">Success response with a list of user tasks.</response>
        /// <response code="401">Unauthorized response with an error message if not authenticated.</response>
        [HttpGet]
        public IActionResult GetTasks()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Not authenticated");
            }

            var userTasks = GetUserTasks(userId);
            return Ok(userTasks);
        }

        /// <summary>
        /// Create a new user task.
        /// </summary>
        /// <param name="userTask">The task to create.</param>
        /// <returns>Returns the created task if successful.</returns>
        /// <response code="201">Created response with the created user task.</response>
        /// <response code="400">Bad Request response with an error message if the task creation fails.</response>
        [HttpPost]
        public IActionResult CreateTask([FromBody] UserTask userTask)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Not authenticated");
            }

            userTask.UserId = userId;

            // Check model state for validation errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest("Task length cannot exceed 256 characters.");
            }

            if (InsertUserTask(userTask, out int newTaskId))
            {
                var createdTask = GetUserTask(newTaskId);

                if (createdTask != null)
                {
                    return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
                }
                else
                {
                    return BadRequest("Failed to create task.");
                }
            }
            else
            {
                return BadRequest("Failed to create task.");
            }
        }

        /// <summary>
        /// Get a user task by its ID.
        /// </summary>
        /// <param name="id">The ID of the task to retrieve.</param>
        /// <returns>Returns the user task with the specified ID if authorized.</returns>
        /// <response code="200">Success response with the user task.</response>
        /// <response code="401">Unauthorized response with an error message if not authenticated.</response>
        /// <response code="403">Forbidden response with an error message if the user doesn't have permission to access this task.</response>
        /// <response code="404">Not Found response with an error message if the task is not found.</response>
        [HttpGet("{id}", Name = "GetTask")]
        public IActionResult GetTask(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Not authenticated");
            }

            var userTask = GetUserTask(id);

            if (userTask == null)
            {
                return NotFound("Task not found");
            }

            if (userTask.UserId != userId)
            {
                return Forbid("You don't have permission to access this task");
            }

            return Ok(userTask);
        }

        /// <summary>
        /// Update a user task by its ID.
        /// </summary>
        /// <param name="id">The ID of the task to update.</param>
        /// <param name="updatedUserTask">The updated task information.</param>
        /// <returns>Returns NoContent if the update is successful.</returns>
        /// <response code="204">No Content response indicating a successful update.</response>
        /// <response code="400">Bad Request response with an error message if the update fails.</response>
        /// <response code="401">Unauthorized response with an error message if not authenticated.</response>
        /// <response code="403">Forbidden response with an error message if the user doesn't have permission to update this task.</response>
        /// <response code="404">Not Found response with an error message if the task is not found.</response>
        [HttpPut("{id}")]
        public IActionResult UpdateTask(int id, [FromBody] UserTask updatedUserTask)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Not authenticated");
            }

            var userTask = GetUserTask(id);

            if (userTask == null)
            {
                return NotFound("Task not found");
            }

            if (userTask.UserId != userId)
            {
                return Forbid("You don't have permission to update this task");
            }

            // Update the task description with the new one
            userTask.TaskDescription = updatedUserTask.TaskDescription;

            // Check model state for validation errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest("Task length cannot exceed 256 characters.");
            }

            if (UpdateUserTask(id, userTask))
            {
                return NoContent();
            }
            else
            {
                return BadRequest("Failed to update task.");
            }
        }

        /// <summary>
        /// Delete a user task by its ID.
        /// </summary>
        /// <param name="id">The ID of the task to delete.</param>
        /// <returns>Returns NoContent if the deletion is successful.</returns>
        /// <response code="204">No Content response indicating a successful deletion.</response>
        /// <response code="400">Bad Request response with an error message if the deletion fails.</response>
        /// <response code="401">Unauthorized response with an error message if not authenticated.</response>
        /// <response code="403">Forbidden response with an error message if the user doesn't have permission to delete this task.</response>
        /// <response code="404">Not Found response with an error message if the task is not found.</response>
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Not authenticated");
            }

            var userTask = GetUserTask(id);

            if (userTask == null)
            {
                return NotFound("Task not found");
            }

            if (userTask.UserId != userId)
            {
                return Forbid("You don't have permission to delete this task");
            }

            if (DeleteUserTask(id))
            {
                return NoContent();
            }
            else
            {
                return BadRequest("Failed to delete task.");
            }
        }

        #region Database Operations

        private List<UserTask> GetUserTasks(string userId)
        {
            var tasks = new List<UserTask>();

            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var command = new MySqlCommand("SELECT * FROM Tasks WHERE UserId = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new UserTask
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserId = reader["UserId"].ToString(),
                                TaskDescription = reader["TaskDescription"].ToString()
                            });
                        }
                    }
                }
            }

            return tasks;
        }

        private UserTask GetUserTask(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var command = new MySqlCommand("SELECT * FROM Tasks WHERE Id = @Id AND UserId = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserTask
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                UserId = reader["UserId"].ToString(),
                                TaskDescription = reader["TaskDescription"].ToString()
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        private bool InsertUserTask(UserTask userTask, out int newTaskId)
        {
            newTaskId = 0;

            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var command = new MySqlCommand("INSERT INTO Tasks (UserId, TaskDescription) VALUES (@UserId, @TaskDescription); SELECT LAST_INSERT_ID();", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userTask.UserId);
                    command.Parameters.AddWithValue("@TaskDescription", userTask.TaskDescription);

                    newTaskId = Convert.ToInt32(command.ExecuteScalar());

                    return newTaskId > 0;
                }
            }
        }

        private bool UpdateUserTask(int id, UserTask updatedUserTask)
        {
            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var command = new MySqlCommand("UPDATE Tasks SET TaskDescription = @TaskDescription WHERE Id = @Id AND UserId = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@UserId", updatedUserTask.UserId);
                    command.Parameters.AddWithValue("@TaskDescription", updatedUserTask.TaskDescription);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        private bool DeleteUserTask(int id)
        {
            using (var connection = _db.GetConnection())
            {
                connection.Open();
                using (var command = new MySqlCommand("DELETE FROM Tasks WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        #endregion
    }
}