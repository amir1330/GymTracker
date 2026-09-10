<script>
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  let date=new Date().toISOString().split('T')[0], notes='', bodyWeight='', items=[], exercises=[], err='';
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Exercises',{headers:{Authorization:'Bearer '+t}});
    if(r.ok) exercises=await r.json();
  });
  function add(){ items=[...items,{exerciseId:exercises[0]?.Id||exercises[0]?.id||0,sets:3,reps:10,weight:null,duration:null}]; }
  async function submit(e){
    e.preventDefault(); err='';
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Workouts',{method:'POST',headers:{Authorization:'Bearer '+t,'Content-Type':'application/json'},body:JSON.stringify({date,notes,bodyWeight:bodyWeight?+bodyWeight:null,exercises:items})});
    if(!r.ok){ err='save failed'; return; }
    goto('/workouts');
  }
</script>
<h2>new workout</h2>
<form on:submit={submit}>
  <div class="form-row">
    <div class="form-group"><label>date</label><input type="date" bind:value={date} required /></div>
    <div class="form-group"><label>body weight</label><input bind:value={bodyWeight} type="number" step="0.1" /></div>
  </div>
  <div class="form-group"><label>notes</label><textarea bind:value={notes}></textarea></div>
  <div class="exercises-section">
    <div class="section-header"><span>exercises</span><button type="button" class="btn btn-small btn-secondary" on:click={add}>+ add</button></div>
    {#each items as it,i}<div class="exercise-entry">
      <select bind:value={it.exerciseId}>{#each exercises as e}<option value={e.Id||e.id}>{e.Name||e.Name}</option>{/each}</select>
      <input type="number" bind:value={it.sets} placeholder="sets" /><input type="number" bind:value={it.reps} placeholder="reps" /><input type="number" step="0.1" bind:value={it.weight} placeholder="kg" />
      <button type="button" class="btn btn-small btn-danger" on:click={()=>items=items.filter((_,j)=>j!==i)}>x</button>
    </div>{/each}
  </div>
  {#if err}<p class="error">{err}</p>{/if}
  <div class="form-actions"><button class="btn btn-primary">save</button><a class="btn btn-secondary" href="/workouts">cancel</a></div>
</form>
