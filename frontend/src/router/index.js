import { createRouter, createWebHistory } from 'vue-router';

import error from './routes/error';
import login from './routes/login';
import signup from './routes/signup';
import management from './routes/management';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...error,
    ...login,
    ...signup,
    ...management
  ]
});

export default router;