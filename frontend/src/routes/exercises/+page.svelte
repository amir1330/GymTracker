<script>
  import { onMount } from 'svelte';
  import { t } from '$lib/i18n/index.js';
  let items=[], loading=true;
  onMount(async ()=>{
    const tok=localStorage.getItem('jwt');
    const r=await fetch('/api/Exercises',{headers:{Authorization:'Bearer '+tok}});
    if(r.ok) items=await r.json();
    loading=false;
  });
  async function del(id){
    const tok=localStorage.getItem('jwt');
    await fetch(`/api/Exercises/${id}`,{method:'DELETE',headers:{Authorization:'Bearer '+tok}});
    items=items.filter(e=>(e.Id||e.id)!==id);
  }
</script>
<div class="exercise-list">
  <div class="header"><h2>{t('exercise.title')}</h2><a href="/exercises/new" class="btn">{t('exercise.new')}</a></div>
  {#if loading}<p>{t('common.loading')}</p>
  {:else}<div class="table-container"><table>
  <thead><tr><th>{t('exercise.name')}</th><th>{t('exercise.group')}</th><th>{t('exercise.type')}</th><th>{t('common.actions')}</th></tr></thead>
  <tbody>{#each items as e}<tr><td>{e.name||e.Name}</td><td><span class="badge">{e.muscleGroup||e.MuscleGroup||''}</span></td><td>{(e.isDuration||e.IsDuration)?t('exercise.duration'):t('exercise.reps')}</td><td class="actions">{#if !(e.isDefault||e.IsDefault)}<a class="btn btn-small" href={`/exercises/${e.Id||e.id}/edit`}>{t('common.edit')}</a> <button class="btn btn-small btn-danger" on:click={()=>del(e.Id||e.id)}>{t('common.delete')}</button>{:else}<span class="muted">{t('common.default')}</span>{/if}</td></tr>{/each}</tbody>
  </table></div>{/if}
</div>
