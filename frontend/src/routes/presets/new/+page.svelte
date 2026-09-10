<script>
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  let name='', items=[], exercises=[], err='';
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Exercises',{headers:{Authorization:'Bearer '+t}});
    if(r.ok) exercises=await r.json();
  });
  function exOf(id){ return exercises.find(e=>String(e.Id||e.id)===String(id)); }
  function add(){ const e=exercises[0]; if(!e) return; items=[...items,{exerciseId:e.Id||e.id,defaultSets:3,defaultReps:10,defaultWeight:null,defaultDuration:null}]; }
  async function submit(e){
    e.preventDefault(); err='';
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Presets',{method:'POST',headers:{Authorization:'Bearer '+t,'Content-Type':'application/json'},body:JSON.stringify({name,exercises:items})});
    if(!r.ok){ err='save failed'; return; }
    goto('/presets');
  }
</script>
<div class="preset-form">
<h2>new preset</h2>
<form on:submit={submit}>
  <div class="form-group"><label for="name">name</label><input type="text" id="name" bind:value={name} name="name" required /></div>
  <div class="exercises-section">
    <div class="section-header"><h3>exercises</h3><button type="button" on:click={add} class="btn btn-secondary">add exercise</button></div>
    {#each items as pe,i}<div class="exercise-entry">
      <select bind:value={pe.exerciseId} name={'exercise'+i} required>{#each exercises as e}<option value={e.Id||e.id}>{e.name||e.Name} ({e.muscleGroup||e.MuscleGroup||''})</option>{/each}</select>
      {#if exOf(pe.exerciseId)&&(exOf(pe.exerciseId).IsDuration||exOf(pe.exerciseId).isDuration)}
        <input type="number" bind:value={pe.defaultDuration} name={'duration'+i} placeholder="duration" min="1" />
      {:else}
        <input type="number" bind:value={pe.defaultSets} name={'sets'+i} placeholder="sets" min="1" />
        <input type="number" bind:value={pe.defaultReps} name={'reps'+i} placeholder="reps" min="1" />
        <input type="number" bind:value={pe.defaultWeight} name={'weight'+i} placeholder="weight" step="0.5" />
      {/if}
      <button type="button" on:click={()=>items=items.filter((_,j)=>j!==i)} class="btn btn-danger btn-small">delete</button>
    </div>{/each}
  </div>
  {#if err}<p class="error">{err}</p>{/if}
  <div class="form-actions"><button type="submit" class="btn">create</button><a href="/presets" class="btn btn-secondary">cancel</a></div>
</form>
</div>
