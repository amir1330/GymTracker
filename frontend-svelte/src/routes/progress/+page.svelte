<script>
  import { onMount } from 'svelte';
  let charts=[];
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Dashboard',{headers:{Authorization:'Bearer '+t}});
    if(r.ok) charts=await r.json();
  });
</script>
<h1>progress</h1>
{#each charts as c}<div>{c.Label||c.label} — {(c.Data?.Points||[]).length} pts</div>{/each}
{#if !charts.length}<p>no charts yet</p>{/if}
