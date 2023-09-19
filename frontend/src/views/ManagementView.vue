<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-100">
    <div class="bg-white p-6 rounded shadow-lg w-96">
      <h1 class="text-3xl font-semibold mb-6">Task Management</h1>

      <!-- Create Task Form -->
      <form @submit.prevent="addTask">
        <div class="mb-4">
          <label class="block text-sm font-semibold mb-2">Task Name</label>
          <input
            v-model="newTaskDescription"
            type="text"
            class="w-full p-2 border rounded focus:outline-none focus:ring focus:border-blue-500"
            placeholder="Enter task name"
          />
        </div>
        <button
          type="submit"
          class="bg-green-500 text-white p-2 rounded hover:bg-green-600"
        >
          Create Task
        </button>
      </form>

      <!-- Task List -->
      <div class="mt-6">
        <ul>
          <li v-for="(task, index) in tasks" :key="task.id" class="mb-2">
            {{ task.taskDescription }}
            <button
              @click="editTask(index)"
              class="ml-2 bg-blue-500 text-white p-1 rounded hover:bg-blue-600"
            >
              Edit
            </button>
            <button
              @click="deleteTask(task.id)"
              class="ml-2 bg-red-500 text-white p-1 rounded hover:bg-red-600"
            >
              Delete
            </button>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      newTaskDescription: "",
      tasks: [],
      editingIndex: -1,
      editingTaskText: "", // New variable to store edited task text
    };
  },
  methods: {
    async addTask() {
      if (this.newTaskDescription.trim() !== "") {
        try {
          const response = await fetch('/api/tasks', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify({
              taskDescription: this.newTaskDescription,
            }),
          });

          if (response.ok) {
            const newTask = await response.json();
            this.tasks.push(newTask);
            this.newTaskDescription = "";
          } else {
            console.error('Failed to create task');
          }
        } catch (error) {
          console.error('An error occurred while creating the task:', error);
        }
      }
    },

    async fetchTasks() {
      try {
        const response = await fetch('/api/tasks', {
          method: 'GET',
        });

        if (response.ok) {
          const tasks = await response.json();
          this.tasks = tasks;
        } else if (response.status === 401) {
          alert('You must be logged in to view this page');
          this.$router.push('/login');
        } else {
          console.error('Failed to fetch tasks');
        }
      } catch (error) {
        console.error('An error occurred while fetching tasks:', error);
      }
    },

    async editTask(index) {
      const taskId = this.tasks[index].id;
      const editedTaskDescription = prompt(
        'Edit Task Description',
        this.tasks[index].taskDescription
      );

      if (editedTaskDescription !== null) {
        try {
          const response = await fetch(`/api/tasks/${taskId}`, {
            method: 'PUT',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify({
              taskDescription: editedTaskDescription,
            }),
          });

          if (response.ok) {
            this.tasks[index].taskDescription = editedTaskDescription;
          } else {
            console.error('Failed to edit task');
          }
        } catch (error) {
          console.error('An error occurred while editing the task:', error);
        }
      }
    },

    async deleteTask(taskId) {
      if (confirm('Are you sure you want to delete this task?')) {
        try {
          const response = await fetch(`/api/tasks/${taskId}`, {
            method: 'DELETE',
          });

          if (response.ok) {
            this.tasks = this.tasks.filter((task) => task.id !== taskId);
          } else {
            console.error('Failed to delete task');
          }
        } catch (error) {
          console.error('An error occurred while deleting the task:', error);
        }
      }
    },
  },

  mounted() {
    this.fetchTasks();
  },
};
</script>