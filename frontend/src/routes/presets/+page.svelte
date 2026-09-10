<script>
  import { onMount } from 'svelte';
  import { t } from '$lib/i18n/index.js';
  let items=[], loading=true;
  onMount(async ()=>{
    const tok=localStorage.getItem('jwt');
    const r=await fetch('/api/Presets',{headers:{Authorization:'Bearer '+tok}});
    if(r.ok) items=await r.json();
    loading=false;
  });
  async function del(id){
    const tok=localStorage.getItem('jwt');
    await fetch(`/api/Presets/${id}`,{method:'DELETE',headers:{Authorization:'Bearer '+tok}});
    items=items.filter(p=>(p.Id||p.id)!==id);
  }
</script>
<div class="preset-list">
  <div class="header"><h2>{t('preset.title')}</h2><a href="/presets/new" class="btn">{t('preset.new')}</a></div>
  {#if loading}<p>{t('common.loading')}</p>
  {:else if !items.length}<p class="empty">{t('preset.empty')}</p>
  {:else}<div class="presets-grid">
  {#each items as p}
    <div class="preset-card"><h3>{p.name||p.Name}</h3><p class="exercise-count">{(p.presetExercises||[]).length} {t('preset.exerciseCount')}</p>
      <div class="preset-exercises">{#each (p.presetExercises||[]) as pe}<span class="exercise-tag">{pe.exercise?.name||''}</span>{/each}</div>
      <div class="preset-actions"><a class="btn btn-secondary" href={`/presets/${p.Id||p.id}/edit`}>{t('common.edit')}</a> <button class="btn btn-danger" on:click={()=>del(p.Id||p.id)}>{t('common.delete')}</button></div>
    </div>
  {/each}
  </div>{/if}
</div>
