/**
 * Cloudflare Worker: AV License Verification & Revocation API
 * AV Civil 3D Tools License Management
 * 
 * Free tier compatible (100,000 requests/day free)
 */

export default {
  async fetch(request, env, ctx) {
    const url = new URL(request.url);
    const corsHeaders = {
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Methods': 'GET, POST, OPTIONS',
      'Access-Control-Allow-Headers': 'Content-Type, Authorization',
      'Access-Control-Max-Age': '86400',
    };

    // Preflight CORS request
    if (request.method === 'OPTIONS') {
      return new Response(null, { status: 204, headers: corsHeaders });
    }

    // Helper: JSON response
    const jsonResponse = (data, status = 200) => {
      return new Response(JSON.stringify(data, null, 2), {
        status,
        headers: {
          'Content-Type': 'application/json; charset=utf-8',
          ...corsHeaders,
        },
      });
    };

    // Helper: Verify Admin Token for administrative actions
    const isAuthorized = (req) => {
      const authHeader = req.headers.get('Authorization') || '';
      const token = authHeader.replace(/^Bearer\s+/i, '').trim();
      const expectedToken = env.ADMIN_SECRET_TOKEN || 'AVAdmin@2026_Secret';
      return token === expectedToken;
    };

    try {
      // 1. Health check & Info
      if (url.pathname === '/' || url.pathname === '/api/health') {
        return jsonResponse({
          service: 'AV License Verification API',
          version: '1.0.0',
          status: 'online',
          timestamp: new Date().toISOString(),
          endpoints: [
            'GET /api/verify?hwid=AV-XXXX-XXXX-XXXX-XXXX',
            'POST /api/revoke',
            'POST /api/activate',
            'GET /api/revoked-list'
          ]
        });
      }

      // 2. Verify License / HWID status
      // Endpoint: GET /api/verify?hwid=AV-XXXX-XXXX-XXXX-XXXX&lic_id=LIC-XXXX
      if (url.pathname === '/api/verify' && request.method === 'GET') {
        const hwid = (url.searchParams.get('hwid') || '').trim().toUpperCase();
        const licId = (url.searchParams.get('lic_id') || '').trim();

        if (!hwid) {
          return jsonResponse({
            status: 'error',
            message: 'Thiếu tham số hwid (Mã phần cứng máy tính)'
          }, 400);
        }

        // Check in Cloudflare KV if bound
        let isRevoked = false;
        let revokeDetails = null;

        if (env.AV_LICENSES) {
          const hwidRecord = await env.AV_LICENSES.get(`revoked:${hwid}`, 'json');
          const licRecord = licId ? await env.AV_LICENSES.get(`revoked:${licId}`, 'json') : null;

          if (hwidRecord) {
            isRevoked = true;
            revokeDetails = hwidRecord;
          } else if (licRecord) {
            isRevoked = true;
            revokeDetails = licRecord;
          }

          // Optional: Record last seen ping asynchronously
          ctx.waitUntil(
            env.AV_LICENSES.put(`ping:${hwid}`, JSON.stringify({
              hwid,
              licId,
              ip: request.headers.get('cf-connecting-ip') || 'unknown',
              country: request.headers.get('cf-ipcountry') || 'VN',
              last_seen: new Date().toISOString()
            }), { expirationTtl: 60 * 60 * 24 * 30 }) // Keep for 30 days
          );
        }

        if (isRevoked) {
          return jsonResponse({
            status: 'revoked',
            hwid,
            lic_id: licId || undefined,
            message: 'Giấy phép đã bị THU HỒI trên hệ thống máy chủ quản trị.',
            revoked_at: revokeDetails?.timestamp || new Date().toISOString(),
            reason: revokeDetails?.reason || 'Thu hồi bởi quản trị viên',
            checked_at: new Date().toISOString()
          });
        }

        return jsonResponse({
          status: 'active',
          hwid,
          lic_id: licId || undefined,
          message: 'Giấy phép HỢP LỆ và đang hoạt động bình thường.',
          checked_at: new Date().toISOString()
        });
      }

      // 3. Admin: Revoke License / HWID
      // Endpoint: POST /api/revoke
      if (url.pathname === '/api/revoke' && request.method === 'POST') {
        if (!isAuthorized(request)) {
          return jsonResponse({ error: 'Unauthorized. Yêu cầu Admin Secret Token.' }, 401);
        }

        const body = await request.json();
        const target = (body.hwid || body.license_id || '').trim().toUpperCase();
        const reason = body.reason || 'Thu hồi bởi quản trị viên AV';

        if (!target) {
          return jsonResponse({ error: 'Thiếu hwid hoặc license_id cần thu hồi' }, 400);
        }

        const revokeRecord = {
          target,
          reason,
          timestamp: new Date().toISOString(),
          revoked_by: body.admin_name || 'Admin'
        };

        if (env.AV_LICENSES) {
          await env.AV_LICENSES.put(`revoked:${target}`, JSON.stringify(revokeRecord));
        }

        return jsonResponse({
          success: true,
          message: `Đã thu hồi thành công ${target}`,
          record: revokeRecord
        });
      }

      // 4. Admin: Activate / Un-revoke HWID
      // Endpoint: POST /api/activate
      if (url.pathname === '/api/activate' && request.method === 'POST') {
        if (!isAuthorized(request)) {
          return jsonResponse({ error: 'Unauthorized. Yêu cầu Admin Secret Token.' }, 401);
        }

        const body = await request.json();
        const target = (body.hwid || body.license_id || '').trim().toUpperCase();

        if (!target) {
          return jsonResponse({ error: 'Thiếu hwid hoặc license_id cần kích hoạt lại' }, 400);
        }

        if (env.AV_LICENSES) {
          await env.AV_LICENSES.delete(`revoked:${target}`);
        }

        return jsonResponse({
          success: true,
          message: `Đã gỡ bỏ thu hồi, kích hoạt lại thành công cho ${target}`,
          target,
          timestamp: new Date().toISOString()
        });
      }

      // 5. Admin: List revoked targets
      // Endpoint: GET /api/revoked-list
      if (url.pathname === '/api/revoked-list' && request.method === 'GET') {
        if (!isAuthorized(request)) {
          return jsonResponse({ error: 'Unauthorized. Yêu cầu Admin Secret Token.' }, 401);
        }

        let list = [];
        if (env.AV_LICENSES) {
          const keys = await env.AV_LICENSES.list({ prefix: 'revoked:' });
          for (const key of keys.keys) {
            const val = await env.AV_LICENSES.get(key.name, 'json');
            if (val) list.push(val);
          }
        }

        return jsonResponse({
          count: list.length,
          revoked_items: list
        });
      }

      return jsonResponse({ error: 'Endpoint không tồn tại' }, 404);
    } catch (err) {
      return jsonResponse({
        error: 'Internal Server Error',
        details: err.message
      }, 500);
    }
  }
};
