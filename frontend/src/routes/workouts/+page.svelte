<script>
  import { onMount } from 'svelte';
  import { t } from '$lib/i18n/index.js';
  let items=[], loading=true;
  onMount(async ()=>{
    const tok=localStorage.getItem('jwt');
    const r=await fetch('/api/Workouts',{headers:{Authorization:'Bearer '+tok}});
    if(r.ok) items=await r.json();
    loading=false;
  });
  async function del(id){
    const tok=localStorage.getItem('jwt');
    await fetch(`/api/Workouts/${id}`,{method:'DELETE',headers:{Authorization:'Bearer '+tok}});
    items=items.filter(w=>(w.Id||w.id)!==id);
  }
  function fmtDate(d){ return (d||'').slice(0,10); }
</script>
<div class="workout-list">
  <div class="header"><h2>{t('workout.title')}</h2><a href="/workouts/new" class="btn">{t('workout.new')}</a></div>
  {#if loading}<p>{t('common.loading')}</p>
  {:else if !items.length}<p class="empty">{t('workout.empty')}</p>
  {:else}<div class="workouts-list">
  {#each items as w}
    <div class="workout-card">
      <div class="workout-header"><span class="workout-date">{fmtDate(w.Date||w.date)}</span>{#if w.BodyWeight||w.bodyWeight}<span class="body-weight">{w.BodyWeight||w.bodyWeight} {t('common.kg')}</span>{/if}</div>
      {#if w.Notes||w.notes}<p class="workout-notes">{w.Notes||w.notes}</p>{/if}
      <div class="workout-exercises">{#each (w.WorkoutExercises||w.workoutExercises||[]) as we}<span class="exercise-tag">{we.exercise?.name||''} {we.sets||we.Sets||''}x{we.reps||we.Reps||''}</span>{/each}</div>
      <div class="workout-actions"><a class="btn btn-secondary" href={`/workouts/${w.Id||w.id}/edit`}>{t('common.edit')}</a><button class="btn btn-danger" on:click={()=>del(w.Id||w.id)}>{t('common.delete')}</button></div>
    </div>
  {/each}
  </div>{/if}
</div>
