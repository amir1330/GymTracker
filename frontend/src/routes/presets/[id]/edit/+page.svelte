<script>
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { page } from '$app/stores';
  let name='', items=[], exercises=[];
  const id=$page.params.id;
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const h={Authorization:'Bearer '+t};
    const er=await fetch('/api/Exercises',{headers:h});
    if(er.ok) exercises=await er.json();
    const r=await fetch(`/api/Presets/${id}`,{headers:h});
    if(r.ok){ const p=await r.json(); name=p.name||p.Name||''; items=(p.presetExercises||p.PresetExercises||[]).map(pe=>({exerciseId:pe.exerciseId||pe.ExerciseId,defaultSets:pe.defaultSets||pe.DefaultSets||3,defaultReps:pe.defaultReps||pe.DefaultReps||10,defaultWeight:pe.defaultWeight||pe.DefaultWeight||null,defaultDuration:pe.defaultDuration||pe.DefaultDuration||null})); }
  });
  function exOf(eid){ return exercises.find(e=>String(e.Id||e.id)===String(eid)); }
  function add(){ const e=exercises[0]; if(!e) return; items=[...items,{exerciseId:e.Id||e.id,defaultSets:3,defaultReps:10,defaultWeight:null,defaultDuration:null}]; }
  async function submit(e){
    e.preventDefault();
    const t=localStorage.getItem('jwt');
    await fetch(`/api/Presets/${id}`,{method:'PUT',headers:{Authorization:'Bearer '+t,'Content-Type':'application/json'},body:JSON.stringify({name,exercises:items})});
    goto('/presets');
  }
</script>
<div class="preset-form">
<h2>edit preset</h2>
<form on:submit={submit}>
  <div class="form-group"><label for="name">name</label><input type="text" id="name" bind:value={name} name="name" required /></div>
  <div class="exercises-section">
    <div class="section-header"><h3>exercises</h3><button type="button" on:click={add} class="btn btn-secondary">add exercise</button></div>
    {#each items as pe,i}<div class="exercise-entry">
      <select bind:value={pe.exerciseId} name={'exercise'+i} required>{#each exercises as e}<option value={e.Id||e.id}>{e.name||e.Name}</option>{/each}</select>
      {#if exOf(pe.exerciseId)&&(exOf(pe.exerciseId).IsDuration||exOf(pe.exerciseId).isDuration)}
        <input type="number" bind:value={pe.defaultDuration} name={'duration'+i} min="1" />
      {:else}
        <input type="number" bind:value={pe.defaultSets} name={'sets'+i} min="1" />
        <input type="number" bind:value={pe.defaultReps} name={'reps'+i} min="1" />
        <input type="number" bind:value={pe.defaultWeight} name={'weight'+i} step="0.5" />
      {/if}
      <button type="button" on:click={()=>items=items.filter((_,j)=>j!==i)} class="btn btn-danger btn-small">delete</button>
    </div>{/each}
  </div>
  <div class="form-actions"><button type="submit" class="btn">update</button><a href="/presets" class="btn btn-secondary">cancel</a></div>
</form>
</div>
