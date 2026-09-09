<script>
  import { onMount } from 'svelte';
  let items=[];
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Workouts',{headers:{Authorization:'Bearer '+t}});
    if(r.ok) items=await r.json();
  });
</script>
<h1>workouts</h1>
<a href="/workouts/new">new</a>
{#each items as w}<div>{w.Date||w.date} — {w.Notes||w.notes||''}</div>{/each}
