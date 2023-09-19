<template>
  <div class="min-h-screen flex items-center justify-center bg-white text-black">
    <div class="bg-white p-8 rounded shadow-lg w-96">
      <h2 class="text-2xl font-bold mb-4">Login</h2>
      <form @submit.prevent="login">
        <div class="mb-4">
          <label class="block text-sm font-semibold mb-2">Username</label>
          <input v-model="username" type="text"
            class="w-full p-2 border rounded focus:outline-none focus:ring focus:border-blue-500"
            placeholder="Your username" />
        </div>
        <div class="mb-4">
          <label class="block text-sm font-semibold mb-2">Password</label>
          <input v-model="password" type="password"
            class="w-full p-2 border rounded focus:outline-none focus:ring focus:border-blue-500"
            placeholder="Your password" />
        </div>
        <button type="submit" class="w-full bg-black text-white p-2 rounded hover:bg-gray-800">
          Login
        </button>
      </form>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      username: '',
      password: '',
    };
  },
  methods: {
    async login() {
      try {
        const response = await fetch('api/auth/login', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            username: this.username,
            password: this.password,
          }),
        });

        if (response.ok) {
          console.log('Login successful');
          this.$router.push('/');
        } else {
          alert('Login failed. Please try again.');
        }
      } catch (error) {
        console.error('An error occurred during login:', error);
      }
    },
  },
};
</script>