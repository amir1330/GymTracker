<script>
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { page } from '$app/stores';
  let date='', notes='', bodyWeight='', items=[];
  const id = $page.params.id;
  onMount(async ()=>{
    const t=localStorage.getItem('jwt');
    const r=await fetch(`/api/Workouts/${id}`,{headers:{Authorization:'Bearer '+t}});
    if(r.ok){ const w=await r.json(); date=(w.Date||w.date||'').slice(0,10); notes=w.Notes||w.notes||''; bodyWeight=w.BodyWeight||w.bodyWeight||''; items=w.WorkoutExercises||w.workoutExercises||w.exercises||[]; }
  });
  async function submit(e){
    e.preventDefault();
    const t=localStorage.getItem('jwt');
    await fetch(`/api/Workouts/${id}`,{method:'PUT',headers:{Authorization:'Bearer '+t,'Content-Type':'application/json'},body:JSON.stringify({date,notes,bodyWeight:bodyWeight?+bodyWeight:null,exercises:items})});
    goto('/workouts');
  }
</script>
<h1>edit workout {id}</h1>
<form on:submit={submit}>
  <input type="date" bind:value={date} required />
  <input bind:value={notes} placeholder="notes" />
  <input bind:value={bodyWeight} placeholder="body weight" type="number" step="0.1" />
  <button>save</button>
</form>
