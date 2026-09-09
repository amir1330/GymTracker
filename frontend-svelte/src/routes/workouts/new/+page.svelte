<script>
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  let date=new Date().toISOString().split('T')[0], notes='', bodyWeight='', items=[], exercises=[];
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch('/api/Exercises',{headers:{Authorization:'Bearer '+t}});
    if(r.ok) exercises=await r.json();
  });
  function add(){ items=[...items,{exerciseId:exercises[0]?.Id||exercises[0]?.id||0,sets:3,reps:10,weight:null,duration:null}]; }
  async function submit(e){
    e.preventDefault();
    const t=localStorage.getItem('jwt');
    await fetch('/api/Workouts',{method:'POST',headers:{Authorization:'Bearer '+t,'Content-Type':'application/json'},body:JSON.stringify({date,notes,bodyWeight:bodyWeight?+bodyWeight:null,exercises:items})});
    goto('/workouts');
  }
</script>
<h1>new workout</h1>
<form on:submit={submit}>
  <input type="date" bind:value={date} required />
  <input bind:value={notes} placeholder="notes" />
  <input bind:value={bodyWeight} placeholder="body weight" type="number" step="0.1" />
  {#each items as it,i}<div>
    <select bind:value={it.exerciseId}>{#each exercises as e}<option value={e.Id||e.id}>{e.Name||e.name}</option>{/each}</select>
    <input type="number" bind:value={it.sets} placeholder="sets" /><input type="number" bind:value={it.reps} placeholder="reps" /><input type="number" step="0.1" bind:value={it.weight} placeholder="kg" />
    <button type="button" on:click={()=>items=items.filter((_,j)=>j!==i)}>x</button>
  </div>{/each}
  <button type="button" on:click={add}>+ exercise</button>
  <button>save</button>
</form>
