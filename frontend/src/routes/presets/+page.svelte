<script>
  import { onMount } from 'svelte';
  let items=[];
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Presets',{headers:{Authorization:'Bearer '+t}});
    if(r.ok) items=await r.json();
  });
  async function del(id){
    const t=localStorage.getItem('jwt');
    await fetch(`/api/Presets/${id}`,{method:'DELETE',headers:{Authorization:'Bearer '+t}});
    items=items.filter(p=>(p.Id||p.id)!==id);
  }
</script>
<div class="header"><h2>presets</h2><a class="btn btn-primary" href="/presets/new">new</a></div>
{#each items as p}
  <div class="preset-card"><span>{p.name||p.Name}</span><span class="workout-actions"><a class="btn btn-small btn-secondary" href={`/presets/${p.Id||p.id}/edit`}>edit</a> <button class="btn btn-small btn-danger" on:click={()=>del(p.Id||p.id)}>del</button></span></div>
{/each}
{#if !items.length}<p class="empty">no presets yet</p>{/if}
