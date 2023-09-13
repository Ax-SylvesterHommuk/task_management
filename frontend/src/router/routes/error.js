import Error from '../../components/Error.vue';

export default [
  {
    path: '/:catchAll(.*)',
    component: Error,
  },
];