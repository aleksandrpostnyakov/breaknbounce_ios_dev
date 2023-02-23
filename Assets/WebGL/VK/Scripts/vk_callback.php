<?php
header("Content-Type: application/json; encoding=utf-8");

$secret_key = 'Yq1qA9F2fLhEydedeK1T'; // Защищенный ключ приложения

$input = $_POST;

// Проверка подписи
$sig = $input['sig'];
unset($input['sig']);
ksort($input);
$str = '';
foreach ($input as $k => $v) {
  $str .= $k.'='.$v;
}

if ($sig != md5($str.$secret_key)) {
  $response['error'] = array(
    'error_code' => 10,
    'error_msg' => 'Несовпадение вычисленной и переданной подписи запроса.',
    'critical' => true
  );
} else {
  // Подпись правильная
  switch ($input['notification_type']) {
    case 'get_item':
      // Получение информации о товаре
      $item = $input['item']; // наименование товара

      if ($item == 'Pack_1') {
        $response['response'] = array(
          'item_id' => 'Pack_1',
          'title' => 'Горстка рубинов',
          'photo_url' => 'http://mergeharvest.a2hosted.com/vk/pics/pack_1.jpg',
          'price' => 15,
          'expiration' = 3000
        );
      } elseif ($item == 'Pack_2') {
        $response['response'] = array(
          'item_id' => 'Pack_2',
          'title' => 'Мешок рубинов',
          'photo_url' => 'http://mergeharvest.a2hosted.com/vk/pics/pack_2.jpg',
          'price' => 35,
          'expiration' = 3000
        );
      } elseif ($item == 'Pack_3') {
        $response['response'] = array(
          'item_id' => 'Pack_3',
          'title' => 'Чаша рубинов',
          'photo_url' => 'http://mergeharvest.a2hosted.com/vk/pics/pack_3.jpg',
          'price' => 69,
          'expiration' = 3000
        );
      } elseif ($item == 'Pack_4') {
        $response['response'] = array(
          'item_id' => 'Pack_4',
          'title' => 'Кубок рубинов',
          'photo_url' => 'http://mergeharvest.a2hosted.com/vk/pics/pack_4.jpg',
          'price' => 135,
          'expiration' = 3000
        );
      } elseif ($item == 'Pack_5') {
        $response['response'] = array(
          'item_id' => 'Pack_5',
          'title' => 'Сундук рубинов',
          'photo_url' => 'http://mergeharvest.a2hosted.com/vk/pics/pack_5.jpg',
          'price' => 335,
          'expiration' = 3000
        );
      } elseif ($item == 'Pack_6') {
        $response['response'] = array(
          'item_id' => 'Pack_6',
          'title' => 'Карета рубинов',
          'photo_url' => 'http://mergeharvest.a2hosted.com/vk/pics/pack_6.jpg',
          'price' => 659,
          'expiration' = 3000
        );
      } elseif ($item == 'gems_specialoffer') {
        $response['response'] = array(
          'item_id' => 'gems_specialoffer',
          'title' => 'Специальное предложение',
          'photo_url' => 'http://mergeharvest.a2hosted.com/vk/pics/special_offer.jpg',
          'price' => 25,
          'expiration' = 3000
        );
      } else {
        $response['error'] = array(
          'error_code' => 20,
          'error_msg' => 'Товара не существует.',
          'critical' => true
        );
      }
      break;

case 'get_item_test':
      // Получение информации о товаре в тестовом режиме
      $item = $input['item'];
      
       if ($item == 'Pack_1') {
          $response['response'] = array(
            'item_id' => 'Pack_1',
            'title' => 'Горстка рубинов (тестовый режим)',
            'photo_url' => 'http://mergeharvest.a2hosted.com/vk/pics/pack_1.jpg',
            'price' => 15
          );
        } elseif ($item == 'Pack_2') {
          $response['response'] = array(
            'item_id' => 'Pack_2',
            'title' => 'Мешок рубинов (тестовый режим)',
            'photo_url' => 'http://mergeharvest.a2hosted.com/vk/pics/pack_2.jpg',
            'price' => 35
          );
        } elseif ($item == 'Pack_3') {
          $response['response'] = array(
            'item_id' => 'Pack_3',
            'title' => 'Чаша рубинов (тестовый режим)',
            'photo_url' => 'http://mergeharvest.a2hosted.com/vk/pics/pack_3.jpg',
            'price' => 69
          );
        } elseif ($item == 'Pack_4') {
          $response['response'] = array(
            'item_id' => 'Pack_4',
            'title' => 'Кубок рубинов (тестовый режим)',
            'photo_url' => 'http://mergeharvest.a2hosted.com/vk/pics/pack_4.jpg',
            'price' => 135
          );
        } elseif ($item == 'Pack_5') {
          $response['response'] = array(
            'item_id' => 'Pack_5',
            'title' => 'Сундук рубинов (тестовый режим)',
            'photo_url' => 'http://mergeharvest.a2hosted.com/vk/pics/pack_5.jpg',
            'price' => 335
          );
        } elseif ($item == 'Pack_6') {
          $response['response'] = array(
            'item_id' => 'Pack_6',
            'title' => 'Карета рубинов (тестовый режим)',
            'photo_url' => 'http://mergeharvest.a2hosted.com/vk/pics/pack_6.jpg',
            'price' => 659
          );
        } elseif ($item == 'gems_specialoffer') {
          $response['response'] = array(
            'item_id' => 'gems_specialoffer',
            'title' => 'Специальное предложение (тестовый режим)',
            'photo_url' => 'http://mergeharvest.a2hosted.com/vk/pics/special_offer.jpg',
            'price' => 25
          );
        } else {
          $response['error'] = array(
            'error_code' => 20,
            'error_msg' => 'Товара не существует.',
            'critical' => true
          );
        }
        break;

case 'order_status_change':
      // Изменение статуса заказа
      if ($input['status'] == 'chargeable') {
        $order_id = intval($input['order_id']);

// Код проверки товара, включая его стоимость
        $app_order_id = random_int(100000, 1000000); // Получающийся у вас идентификатор заказа.

$response['response'] = array(
          'order_id' => $order_id,
          'app_order_id' => $app_order_id,
        );
      } else {
        $response['error'] = array(
          'error_code' => 100,
          'error_msg' => 'Передано непонятно что вместо chargeable.',
          'critical' => true
        );
      }
      break;

case 'order_status_change_test':
      // Изменение статуса заказа в тестовом режиме
      if ($input['status'] == 'chargeable') {
        $order_id = intval($input['order_id']);

$app_order_id = 1; // Тут фактического заказа может не быть - тестовый режим.

$response['response'] = array(
          'order_id' => $order_id,
          'app_order_id' => $app_order_id,
        );
      } else {
        $response['error'] = array(
          'error_code' => 100,
          'error_msg' => 'Передано непонятно что вместо chargeable.',
          'critical' => true
        );
      }
      break;
  }
}

echo json_encode($response);
?>