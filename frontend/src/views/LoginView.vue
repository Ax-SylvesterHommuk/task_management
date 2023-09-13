<template>
  <div class="min-h-screen flex items-center justify-center bg-white text-black">
    <div class="bg-white p-8 rounded shadow-lg w-96">
      <h2 class="text-2xl font-bold mb-4">Login</h2>
      <form>
        <div class="mb-4">
          <label class="block text-sm font-semibold mb-2">Email</label>
          <input
            type="email"
            class="w-full p-2 border rounded focus:outline-none focus:ring focus:border-blue-500"
            placeholder="Your email"
          />
        </div>
        <div class="mb-4">
          <label class="block text-sm font-semibold mb-2">Password</label>
          <input
            type="password"
            class="w-full p-2 border rounded focus:outline-none focus:ring focus:border-blue-500"
            placeholder="Your password"
          />
        </div>
        <button
          type="submit"
          class="w-full bg-black text-white p-2 rounded hover:bg-gray-800"
        >
          Login
        </button>
      </form>
    </div>
  </div>
</template>

<script>
</script>