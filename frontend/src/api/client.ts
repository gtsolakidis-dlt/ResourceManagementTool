import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5212/api',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': 'Basic ' + btoa('admin:Deloitte2026!')
  },
});

export default api;
