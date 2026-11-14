# GHOST — Tàn tích cổ

Game hành động platformer 2D pixel-art, có bản trình duyệt Canvas, bản Unity hien tai chua hoan thien. Phần nền hiện có được giữ nguyên; địa hình, cảnh quan tương tác và đấu trường cuối màn được thiết kế lại theo chủ đề tàn tích giả tưởng. Bản web có các vùng địa hình rừng rậm, phế tích, tinh thể, đầm lầy và hang đá. Nhân vật chính là quả cầu đen với đôi mắt trắng; tiền tệ trong game là đồng vàng.

## Bản trình duyệt

Mở `index.html`, hoặc chạy máy chủ local trong thư mục dự án bằng `python -m http.server 8000`, sau đó truy cập <http://localhost:8000>. Bản Canvas không cần thư viện ngoài.

### Điều khiển

- **A/D** hoặc **←/→**: di chuyển; **Space**: nhảy.
- **J** hoặc **X**: nhấn liên tiếp để tung combo 3 đòn; đòn thứ ba mạnh hơn và tốn thể lực hơn.
- **Shift**: lướt né đòn, tiêu hao 24 thể lực.
- **H**: dùng 1 trong 3 bình hồi máu, hồi tối đa 4 máu.
- Linh vật thường hồi sinh sau **20 giây**.
- Đến gần **thương nhân** ở đầu màn và nhấn **E** (hoặc chọn nút hiện trên màn hình) để dùng vàng nâng cấp máu, sát thương hoặc thể lực. Mỗi chỉ số nâng tối đa 3 lần; giá lần lượt là 8/16/24 vàng cho máu và thể lực, 12/24/36 vàng cho sát thương.
- Tại **đền an toàn** gần đấu trường boss, nhấn **E** để hồi đầy máu, thể lực, bình máu và lưu điểm hồi sinh. Nếu Cua Đột Biến hạ Ghost, bạn hồi sinh tại đền và được cấp lại bình máu.
- **M**: bật/tắt bản đồ.
- Đánh tiêu hao thể lực; ngừng đánh để thể lực hồi lại. Quái báo trước đòn để người chơi né rồi phản công.
- Máu tối đa ban đầu là 10; khi hết máu, Ghost hồi sinh tại điểm kiểm soát gần nhất.
- Tìm **3 chìa khóa cổ**, đánh bại Cua Đột Biến ở đấu trường cuối màn, rồi đi qua cổng thoát. Kẻ địch rơi đồng vàng.
- Boss bản web là **Cua Đột Biến** với 48 máu và 2 giai đoạn; khi còn nửa máu, boss chuyển sang giai đoạn nhanh và nguy hiểm hơn.

## Chạy game

Thương nhân và các nâng cấp hiện chỉ có trong bản trình duyệt.

1. Mở thư mục này bằng Unity 6 (hoặc bản Unity tương thích).
2. Chờ Unity biên dịch script. Scene `Assets/GhostRiver/Scenes/Ghost_Level_1.unity` được tạo và mở tự động; cũng có thể dùng menu **Ghost > Tạo hoặc mở Màn 1**.
3. Nhấn **Play**.

### Điều khiển Unity

- **A/D** hoặc **←/→**: di chuyển.
- **Space**, **W** hoặc **↑**: nhảy.
- **Shift**: lướt.
- **J/X** hoặc chuột trái: đánh combo 3 đòn.
- **H**: dùng bình hồi máu; **M**: bật/tắt bản đồ; **Esc**: tạm dừng hoặc đóng Mục lục.
- Trên thiết bị cảm ứng, các nút di chuyển, nhảy, lướt và đánh hiện ở cạnh dưới màn hình.
- Tìm đủ **3 chìa khóa cổ**, hạ boss canh cổng và đi qua lối thoát.
- Rơi khỏi địa hình hoặc hết máu sẽ đưa Ghost về điểm kiểm soát gần nhất.

Scene Unity bố trí địa hình và cảnh quan bằng code. Sprite pixel-art được tạo bởi `Tools/generate_pixel_art.py` (chỉ dùng thư viện chuẩn Python), lưu trong `Assets/GhostRiver/Resources/GhostRiverArt/` và tự nhập với point-filter để giữ nét sắc; không cần tải asset ngoài. Cả hai bản có nhân vật quả cầu đen mắt trắng, đồng vàng, ba loại kẻ địch và trận boss cuối màn.
