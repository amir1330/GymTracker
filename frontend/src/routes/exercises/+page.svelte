<script>
  import { onMount } from 'svelte';
  let items=[];
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Exercises',{headers:{Authorization:'Bearer '+t}});
    if(r.ok) items=await r.json();
  });
  async function del(id){
    const t=localStorage.getItem('jwt');
    await fetch(`/api/Exercises/${id}`,{method:'DELETE',headers:{Authorization:'Bearer '+t}});
    items=items.filter(e=>(e.Id||e.id)!==id);
  }
</script>
<div class="header"><h2>exercises</h2><a class="btn btn-primary" href="/exercises/new">new</a></div>
<div class="table-container"><table>
<thead><tr><th>name</th><th>muscle</th><th class="actions">actions</th></tr></thead>
<tbody>{#each items as e}<tr><td>{e.name||e.Name}</td><td class="muted">{e.muscleGroup||e.MuscleGroup||''}</td><td class="actions"><a class="btn btn-small btn-secondary" href={`/exercises/${e.Id||e.id}/edit`}>edit</a> <button class="btn btn-small btn-danger" on:click={()=>del(e.Id||e.id)}>del</button></td></tr>{/each}</tbody>
</table></div>
{#if !items.length}<p class="empty">no exercises yet</p>{/if}
