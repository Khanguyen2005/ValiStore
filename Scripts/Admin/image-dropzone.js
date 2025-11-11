(function(){
 function initZone(zone){
 var input = zone.querySelector('input[type=file]');
 if(!input) return;
 var preview = zone.querySelector('[data-preview]');
 var textBox = zone.querySelector('.dz-text');
 function show(file){
 if(!file) return;
 var reader = new FileReader();
 reader.onload = function(e){
 if(preview){ preview.src = e.target.result; preview.classList.remove('d-none'); }
 if(textBox){ textBox.classList.add('d-none'); }
 };
 reader.readAsDataURL(file);
 }
 input.addEventListener('change', function(){ if(input.files && input.files[0]) show(input.files[0]); });
 ['dragenter','dragover'].forEach(function(ev){ zone.addEventListener(ev,function(e){ e.preventDefault(); e.stopPropagation(); zone.classList.add('border-primary'); }); });
 ['dragleave','drop'].forEach(function(ev){ zone.addEventListener(ev,function(e){ e.preventDefault(); e.stopPropagation(); zone.classList.remove('border-primary'); }); });
 zone.addEventListener('drop', function(e){ if(e.dataTransfer && e.dataTransfer.files && e.dataTransfer.files[0]){ input.files = e.dataTransfer.files; show(input.files[0]); }});
 }
 document.addEventListener('DOMContentLoaded', function(){
 var zones = document.querySelectorAll('.img-dropzone');
 for(var i=0;i<zones.length;i++){ initZone(zones[i]); }
 });
})();