const buttons = document.querySelectorAll('.btn-add')

function showToast(messenge) {
    const toast = document.createElement('div')

    toast.innerText = messenge;
    toast.style.backgroundColor = '#333';
    toast.style.color = 'white';
    toast.style.padding = '10px 20px';
    toast.style.borderRadius = '5px';
    toast.style.zIndex = '1000'; // Đảm bảo luôn nằm trên cùng

    document.getElementById('toast-container').appendChild(toast);

    setTimeout(() => {
        toast.remove();
    }, 3000);
}



buttons.forEach(btn => {
    btn.addEventListener('click', (e) => {
        const card = e.target.closest('.product-card');

        const productId = card.dataset.id;
        const productName = card.dataset.name;

        //alert(`Khang đã chọn sản phẩm: ${productName} (ID: ${productId})`);
        let btncolor = e.target;

        btncolor.style.backgroundColor = 'rgba(12, 255, 121, 1)';

        let stock = card.querySelector('.stock-count');
        let currentStock = parseInt(stock.innerText);
        let newstock = currentStock - 1;
        stock.innerText = newstock;

        const price = card.querySelector('.price').innerText;

        showToast(`Đã thêm ${productName} vào giỏ hàng. Giá: ${price}`);

        if (newstock <= 0) {
            stock.innerText = '0';
            btncolor.disabled = true;
            btncolor.innerText = 'Hết hàng';
            btncolor.style.backgroundColor = 'rgba(255, 0, 0, 1)';
        }
    })
})



