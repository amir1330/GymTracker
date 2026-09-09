<script>
  import { onMount } from 'svelte';
  let items=[];
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Exercises',{headers:{Authorization:'Bearer '+t}});
    if(r.ok) items=await r.json();
  });
</script>
<h1>exercises</h1>
{#each items as e}<div>{e.Name||e.name} — {e.MuscleGroup||e.muscleGroup||''}</div>{/each}
