<script>
  import { onMount } from 'svelte';
  let items=[];
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Workouts',{headers:{Authorization:'Bearer '+t}});
    if(r.ok) items=await r.json();
  });
  async function del(id){
    const t=localStorage.getItem('jwt');
    await fetch(`/api/Workouts/${id}`,{method:'DELETE',headers:{Authorization:'Bearer '+t}});
    items=items.filter(w=>(w.Id||w.id)!==id);
  }
</script>
<div class="header"><h2>workouts</h2><a class="btn btn-primary" href="/workouts/new">new</a></div>
{#each items as w}
  <div class="workout-card">
    <div class="workout-header"><span>{(w.Date||w.date||'').slice(0,10)}</span><span class="muted">{w.Notes||w.notes||''}</span></div>
    <div class="actions"><a class="btn btn-small btn-secondary" href={`/workouts/${w.Id||w.id}/edit`}>edit</a><button class="btn btn-small btn-danger" on:click={()=>del(w.Id||w.id)}>del</button></div>
  </div>
{/each}
{#if !items.length}<p class="empty">no workouts yet</p>{/if}
