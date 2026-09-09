<script>
  import { onMount } from 'svelte';
  let items=[];
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Presets',{headers:{Authorization:'Bearer '+t}});
    if(r.ok) items=await r.json();
  });
</script>
<h1>presets</h1>
{#each items as p}<div>{p.Name||p.name}</div>{/each}
