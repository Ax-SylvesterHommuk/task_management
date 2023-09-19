using Microsoft.AspNetCore.Mvc;
using task_backend.Models;
using System.Data;
using System.Data.SQLite;

namespace task_backend.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class UserTasksController : ControllerBase
    {
        private static readonly string ConnectionString = "Data Source=mydatabase.db;Version=3;";

        // GET /api/tasks
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

        // POST /api/tasks
        [HttpPost]
        public IActionResult CreateTask([FromBody] UserTask userTask)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Not authenticated");
            }

            userTask.UserId = userId;

            if (InsertUserTask(userTask))
            {
                return CreatedAtAction(nameof(GetTask), new { id = userTask.Id }, userTask);
            }
            else
            {
                return BadRequest("Failed to create task.");
            }
        }

        // GET /api/tasks/{id}
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

        // PUT /api/tasks/{id}
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

            if (UpdateUserTask(id, userTask))
            {
                return NoContent();
            }
            else
            {
                return BadRequest("Failed to update task.");
            }
        }

        // DELETE /api/tasks/{id}
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

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand("SELECT * FROM Tasks WHERE UserId = @UserId", connection))
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

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand("SELECT * FROM Tasks WHERE Id = @Id AND UserId = @UserId", connection))
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

        private bool InsertUserTask(UserTask userTask)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand("INSERT INTO Tasks (UserId, TaskDescription) VALUES (@UserId, @TaskDescription)", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userTask.UserId);
                    command.Parameters.AddWithValue("@TaskDescription", userTask.TaskDescription);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        private bool UpdateUserTask(int id, UserTask updatedUserTask)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand("UPDATE Tasks SET TaskDescription = @TaskDescription WHERE Id = @Id AND UserId = @UserId", connection))
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
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand("DELETE FROM Tasks WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        #endregion
    }
}