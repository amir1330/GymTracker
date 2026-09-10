<script>
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  let date=new Date().toISOString().split('T')[0], notes='', bodyWeight='', items=[], exercises=[], presets=[], presetId='', err='';
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const h={Authorization:'Bearer '+t};
    const r=await fetch('/api/Exercises',{headers:h});
    if(r.ok) exercises=await r.json();
    const p=await fetch('/api/Presets',{headers:h});
    if(p.ok) presets=await p.json();
  });
  function exOf(id){ return exercises.find(e=>(e.Id||e.id)===+id); }
  function add(){
    const e=exercises[0]; if(!e) return;
    const dur=e.IsDuration||e.isDuration;
    items=[...items,{exerciseId:e.Id||e.id,sets:dur?0:3,reps:dur?0:10,weight:null,duration:dur?30:null,durationUnit:0}];
  }
  function onType(it){
    const e=exOf(it.exerciseId);
    if(e&&(e.IsDuration||e.isDuration)){ it.sets=0; it.reps=0; it.weight=null; if(!it.duration) it.duration=30; }
    else { it.duration=null; if(!it.sets) it.sets=3; if(!it.reps) it.reps=10; }
    items=[...items];
  }
  function loadPreset(){
    const p=presets.find(x=>String(x.Id||x.id)===String(presetId)); if(!p) return;
    items=(p.presetExercises||p.PresetExercises||[]).map(pe=>({exerciseId:pe.exerciseId||pe.ExerciseId,sets:pe.defaultSets||pe.DefaultSets||3,reps:pe.defaultReps||pe.DefaultReps||10,weight:pe.defaultWeight||pe.DefaultWeight||null,duration:pe.defaultDuration||pe.DefaultDuration||null,durationUnit:0}));
  }
  function unitLabel(u){ return u===1?'minutes':u===2?'hours':'seconds'; }
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
  <div class="form-group"><label>preset</label><div class="preset-buttons"><select bind:value={presetId} on:change={loadPreset}><option value="">—</option>{#each presets as p}<option value={p.Id||p.id}>{p.name||p.Name}</option>{/each}</select></div></div>
  <div class="exercises-section">
    <div class="section-header"><span>exercises</span><button type="button" class="btn btn-small btn-secondary" on:click={add}>+ add</button></div>
    {#each items as it,i}<div class="exercise-entry">
      <select bind:value={it.exerciseId} on:change={()=>onType(it)}>{#each exercises as e}<option value={e.Id||e.id}>{e.name||e.Name}</option>{/each}</select>
      {#if exOf(it.exerciseId)&&(exOf(it.exerciseId).IsDuration||exOf(it.exerciseId).isDuration)}
        <input type="number" min="1" bind:value={it.duration} placeholder="duration" /><select class="duration-unit" bind:value={it.durationUnit}><option value={0}>seconds</option><option value={1}>minutes</option><option value={2}>hours</option></select>
      {:else}
        <input type="number" min="1" bind:value={it.sets} placeholder="sets" /><input type="number" min="1" bind:value={it.reps} placeholder="reps" /><input type="number" step="0.1" bind:value={it.weight} placeholder="kg" />
      {/if}
      <button type="button" class="btn btn-small btn-danger" on:click={()=>items=items.filter((_,j)=>j!==i)}>x</button>
    </div>{/each}
  </div>
  {#if err}<p class="error">{err}</p>{/if}
  <div class="form-actions"><button class="btn btn-primary">save</button><a class="btn btn-secondary" href="/workouts">cancel</a></div>
</form>
