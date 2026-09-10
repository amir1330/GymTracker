<script>
  import '../app.css';
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { page } from '$app/stores';
  let token = null;
  onMount(() => {
    token = localStorage.getItem('jwt');
    const pub = ['/login', '/register'];
    if (!token && !pub.includes($page.url.pathname)) goto('/login');
  });
  function logout() { localStorage.removeItem('jwt'); goto('/login'); }
</script>
<div class="app">
<nav class="nav">
  <div class="nav-brand"><a class="brand" href="/progress">gym</a></div>
  <div class="nav-links">
    <a class="nav-link" href="/progress">progress</a><a class="nav-link" href="/workouts">workouts</a><a class="nav-link" href="/exercises">exercises</a><a class="nav-link" href="/presets">presets</a><a class="nav-link" href="/settings">settings</a>
  </div>
  {#if token}<a class="nav-link" href="#" on:click|preventDefault={logout}>logout</a>{:else}<a class="nav-link" href="/login">login</a>{/if}
</nav>
<main class="main"><slot /></main>
</div>
