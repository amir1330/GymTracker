<script>
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
<style>
  :root{--bg:#282828;--fg:#ebdbb2;--green:#b8bb26;--blue:#83a598}
  nav{display:flex;gap:12px;padding:10px;background:#1d2021;font-family:monospace}
  a{color:var(--blue);text-decoration:none}
  main{max-width:900px;margin:0 auto;padding:12px;font-family:monospace}
</style>
<nav>
  <a href="/progress">progress</a><a href="/workouts">workouts</a><a href="/exercises">exercises</a><a href="/presets">presets</a><a href="/settings">settings</a>
  <span style="flex:1"></span>
  {#if token}<a href="#" on:click|preventDefault={logout}>logout</a>{:else}<a href="/login">login</a>{/if}
</nav>
<main><slot /></main>
