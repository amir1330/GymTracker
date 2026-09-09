<script>
  export let points=[];
  export let label='';
  import { onMount } from 'svelte';
  let cv;
  onMount(()=>{
    const ctx=cv.getContext('2d');
    ctx.strokeStyle='#665c54'; ctx.fillStyle='#a89984'; ctx.font='10px monospace';
    const W=cv.width,H=cv.height;
    ctx.strokeRect(0.5,0.5,W-1,H-1);
    if(!points.length) return;
    const max=Math.max(...points.map(p=>p.value),1);
    ctx.strokeStyle='#b8bb26'; ctx.beginPath();
    points.forEach((p,i)=>{
      const x=10+(W-20)*i/Math.max(points.length-1,1);
      const y=H-10-(H-20)*(p.value/max);
      i?ctx.lineTo(x,y):ctx.moveTo(x,y);
    });
    ctx.stroke();
  });
</script>
<div><b>{label}</b><br /><canvas bind:this={cv} width="300" height="120"></canvas></div>
