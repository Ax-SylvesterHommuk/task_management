<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-100">
    <div class="bg-white p-6 rounded shadow-lg w-96">
      <h1 class="text-3xl font-semibold mb-6">Task Management</h1>

      <!-- Create Task Form -->
      <form @submit.prevent="addTask">
        <div class="mb-4">
          <label class="block text-sm font-semibold mb-2">Task Name</label>
          <input
            v-model="newTask"
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
          <li v-for="(task, index) in tasks" :key="index" class="mb-2">
            {{ task }}
            <button
              @click="editTask(index)"
              class="ml-2 bg-blue-500 text-white p-1 rounded hover:bg-blue-600"
            >
              Edit
            </button>
            <button
              @click="deleteTask(index)"
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
      newTask: "",
      tasks: [],
      editingIndex: -1,
      editingTaskText: "", // New variable to store edited task text
    };
  },
  methods: {
    addTask() {
      if (this.newTask.trim() !== "") {
        if (this.editingIndex === -1) {
          this.tasks.push(this.newTask);
        } else {
          this.tasks[this.editingIndex] = this.newTask;
          this.editingIndex = -1;
        }
        this.newTask = "";
      }
    },
    editTask(index) {
      this.editingTaskText = this.tasks[index]; // Store the task text
      this.editingIndex = index;
      const editedTask = prompt("Edit Task Name", this.tasks[index]);
      if (editedTask !== null) {
        this.tasks[index] = editedTask;
      }
      this.editingIndex = -1;
      this.editingTaskText = ""; // Clear the temporary variable
    },
    deleteTask(index) {
      if (confirm("Are you sure you want to delete this task?")) {
        this.tasks.splice(index, 1);
        if (this.editingIndex === index) {
          this.newTask = "";
          this.editingIndex = -1;
        }
      }
    },
  },
};
</script>